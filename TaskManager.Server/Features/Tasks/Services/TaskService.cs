using Microsoft.EntityFrameworkCore;
using TaskManager.Server.Common.Constants;
using TaskManager.Server.Data;
using TaskManager.Server.Enums.Tasks;
using TaskManager.Server.Features.Tasks.DTOs;
using TaskManager.Server.Models.Tasks;
using TaskManager.Server.Services.CurrentUser;
using TaskStatusEnum = TaskManager.Server.Enums.Tasks.TaskStatus;

namespace TaskManager.Server.Features.Tasks.Services;

public sealed class TaskService(
    ApplicationDbContext db,
    ICurrentUserService currentUser)
    : ITaskService
{
    public async Task<IReadOnlyCollection<TaskListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();

        IQueryable<TaskItem> query = db.Tasks.AsNoTracking();

        if (await currentUser.HasPermissionAsync(PermissionCodes.TasksViewAll, cancellationToken))
        {
            // unrestricted
        }
        else if (await currentUser.HasPermissionAsync(PermissionCodes.TasksViewDepartment, cancellationToken))
        {
            var departmentId = await db.Users
                .Where(x => x.Id == userId)
                .Select(x => x.DepartmentId)
                .FirstOrDefaultAsync(cancellationToken);

            query = departmentId.HasValue
                ? query.Where(x => x.AssignedTo.DepartmentId == departmentId.Value)
                : query.Where(x => x.AssignedToId == userId || x.CreatedById == userId);
        }
        else
        {
            query = query.Where(x => x.AssignedToId == userId || x.CreatedById == userId);
        }

        return await query
            .OrderByDescending(x => x.IsImportant)
            .ThenBy(x => x.DueDate)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new TaskListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Status = x.Status,
                Priority = x.Priority,
                IsImportant = x.IsImportant,
                Progress = x.Progress,
                StartDate = x.StartDate,
                DueDate = x.DueDate,
                CompletedAt = x.CompletedAt,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                CreatedById = x.CreatedById,
                CreatedByName = x.CreatedBy.DisplayName,
                AssignedToId = x.AssignedToId,
                AssignedToName = x.AssignedTo.DisplayName,
                AssignedToDepartmentName = x.AssignedTo.Department != null
                    ? x.AssignedTo.Department.Name
                    : null,
                CommentCount = x.Comments.Count()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var task = await db.Tasks
            .AsNoTracking()
            .Include(x => x.CreatedBy)
            .Include(x => x.AssignedTo)
                .ThenInclude(x => x.Department)
            .Include(x => x.Comments)
                .ThenInclude(x => x.Author)
            .Include(x => x.History)
                .ThenInclude(x => x.ChangedBy)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (task is null || !await CanAccessAsync(task, cancellationToken))
            return null;

        return MapDetails(task);
    }

    public async Task<TaskDto> CreateAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();

        if (!await currentUser.HasPermissionAsync(PermissionCodes.TasksCreate, cancellationToken))
            throw new UnauthorizedAccessException("Недостаточно прав для создания задач.");

        ValidateTitle(request.Title);
        ValidateDates(request.StartDate, request.DueDate);
        ValidateEnum(request.Priority);

        var assignedToId = await ResolveAssignmentAsync(
            request.AssignedToId,
            userId,
            cancellationToken);

        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = Normalize(request.Description),
            Status = TaskStatusEnum.New,
            Priority = request.Priority,
            IsImportant = request.IsImportant,
            Progress = 0,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedById = userId,
            AssignedToId = assignedToId
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);

        if (assignedToId != userId)
        {
            db.TaskHistory.Add(new TaskHistory
            {
                TaskItemId = task.Id,
                ChangedById = userId,
                NewAssignedToId = assignedToId,
                NewStatus = TaskStatusEnum.New,
                Comment = "Задача создана и назначена сотруднику."
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        return (await GetByIdAsync(task.Id, cancellationToken))!;
    }

    public async Task<TaskDto?> UpdateAsync(
        int id,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();

        if (!await currentUser.HasPermissionAsync(PermissionCodes.TasksEdit, cancellationToken))
            throw new UnauthorizedAccessException("Недостаточно прав для редактирования задач.");

        var task = await db.Tasks
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (task is null || !await CanAccessAsync(task, cancellationToken))
            return null;

        ValidateTitle(request.Title);
        ValidateDates(request.StartDate, request.DueDate);
        ValidateEnum(request.Priority);

        var assignedToId = await ResolveAssignmentAsync(
            request.AssignedToId,
            userId,
            cancellationToken);

        var changed = false;
        var history = new TaskHistory
        {
            TaskItemId = task.Id,
            ChangedById = userId,
            OldStatus = task.Status,
            NewStatus = task.Status,
            OldPriority = task.Priority,
            NewPriority = task.Priority,
            OldAssignedToId = task.AssignedToId,
            NewAssignedToId = assignedToId,
            OldDueDate = task.DueDate,
            NewDueDate = request.DueDate
        };

        if (task.Title != request.Title.Trim()) { task.Title = request.Title.Trim(); changed = true; }
        var description = Normalize(request.Description);
        if (task.Description != description) { task.Description = description; changed = true; }
        if (task.Priority != request.Priority) { task.Priority = request.Priority; changed = true; }
        if (task.IsImportant != request.IsImportant) { task.IsImportant = request.IsImportant; changed = true; }
        if (task.Progress != request.Progress) { task.Progress = Math.Clamp(request.Progress, 0, 100); changed = true; }
        if (task.StartDate != request.StartDate) { task.StartDate = request.StartDate; changed = true; }
        if (task.DueDate != request.DueDate) { task.DueDate = request.DueDate; changed = true; }
        if (task.AssignedToId != assignedToId) { task.AssignedToId = assignedToId; changed = true; }

        if (!changed)
            return MapDetails(await LoadTaskAsync(id, cancellationToken));

        if (task.Progress >= 100 && task.Status != TaskStatusEnum.Completed)
            task.Status = TaskStatusEnum.Completed;

        if (task.Status == TaskStatusEnum.Completed)
        {
            task.Progress = 100;
            task.CompletedAt ??= DateTime.UtcNow;
        }
        else if (task.CompletedAt.HasValue)
        {
            task.CompletedAt = null;
        }

        task.UpdatedAt = DateTime.UtcNow;
        history.NewStatus = task.Status;
        history.NewPriority = task.Priority;

        db.TaskHistory.Add(history);
        await db.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(id, cancellationToken))!;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();

        if (!await currentUser.HasPermissionAsync(PermissionCodes.TasksDelete, cancellationToken))
            throw new UnauthorizedAccessException("Недостаточно прав для удаления задач.");

        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null)
            return false;

        if (!await CanAccessAsync(task, cancellationToken))
            return false;

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
        _ = userId;
        return true;
    }

    public async Task<TaskDto?> ChangeStatusAsync(
        int id,
        ChangeTaskStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();

        if (!await currentUser.HasPermissionAsync(PermissionCodes.TasksEdit, cancellationToken))
            throw new UnauthorizedAccessException("Недостаточно прав для изменения статуса.");

        ValidateEnum(request.Status);

        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null || !await CanAccessAsync(task, cancellationToken))
            return null;

        var oldStatus = task.Status;
        var oldProgress = task.Progress;

        task.Status = request.Status;
        if (request.Progress.HasValue)
            task.Progress = Math.Clamp(request.Progress.Value, 0, 100);
        else if (request.Status == TaskStatusEnum.Completed)
            task.Progress = 100;
        else if (request.Status == TaskStatusEnum.New)
            task.Progress = 0;

        task.CompletedAt = request.Status == TaskStatusEnum.Completed
            ? (task.CompletedAt ?? DateTime.UtcNow)
            : null;
        task.UpdatedAt = DateTime.UtcNow;

        db.TaskHistory.Add(new TaskHistory
        {
            TaskItemId = task.Id,
            ChangedById = userId,
            OldStatus = oldStatus,
            NewStatus = task.Status,
            OldPriority = task.Priority,
            NewPriority = task.Priority,
            OldDueDate = task.DueDate,
            NewDueDate = task.DueDate,
            Comment = request.Comment
        });

        await db.SaveChangesAsync(cancellationToken);
        _ = oldProgress;
        return (await GetByIdAsync(id, cancellationToken))!;
    }

    public async Task<TaskDto?> ChangePriorityAsync(
        int id,
        ChangeTaskPriorityRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();

        if (!await currentUser.HasPermissionAsync(PermissionCodes.TasksEdit, cancellationToken))
            throw new UnauthorizedAccessException("Недостаточно прав для изменения приоритета.");

        ValidateEnum(request.Priority);

        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null || !await CanAccessAsync(task, cancellationToken))
            return null;

        var oldPriority = task.Priority;
        if (oldPriority == request.Priority)
            return (await GetByIdAsync(id, cancellationToken))!;

        task.Priority = request.Priority;
        task.UpdatedAt = DateTime.UtcNow;

        db.TaskHistory.Add(new TaskHistory
        {
            TaskItemId = task.Id,
            ChangedById = userId,
            OldStatus = task.Status,
            NewStatus = task.Status,
            OldPriority = oldPriority,
            NewPriority = request.Priority,
            OldDueDate = task.DueDate,
            NewDueDate = task.DueDate,
            Comment = request.Comment
        });

        await db.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(id, cancellationToken))!;
    }

    public async Task<TaskCommentDto?> AddCommentAsync(
        int id,
        CreateTaskCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();
        var text = request.Text.Trim();

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Комментарий не может быть пустым.");

        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null || !await CanAccessAsync(task, cancellationToken))
            return null;

        var comment = new TaskComment
        {
            TaskItemId = id,
            AuthorId = userId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        db.TaskComments.Add(comment);
        task.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return await db.TaskComments
            .AsNoTracking()
            .Where(x => x.Id == comment.Id)
            .Select(x => new TaskCommentDto
            {
                Id = x.Id,
                AuthorId = x.AuthorId,
                AuthorName = x.Author.DisplayName,
                Text = x.Text,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstAsync(cancellationToken);
    }

    private async Task<int> ResolveAssignmentAsync(
        int requestedId,
        int currentUserId,
        CancellationToken cancellationToken)
    {
        if (requestedId <= 0)
            throw new ArgumentException("Необходимо указать исполнителя.");

        var target = await db.Users
            .AsNoTracking()
            .Where(x => x.Id == requestedId && x.IsActive)
            .Select(x => new { x.Id, x.DepartmentId })
            .FirstOrDefaultAsync(cancellationToken);

        if (target is null)
            throw new KeyNotFoundException("Исполнитель не найден или деактивирован.");

        if (requestedId == currentUserId)
            return requestedId;

        if (!await currentUser.HasPermissionAsync(PermissionCodes.TasksAssign, cancellationToken))
            throw new UnauthorizedAccessException("Недостаточно прав для назначения задачи другому сотруднику.");

        if (await currentUser.HasPermissionAsync(PermissionCodes.TasksViewAll, cancellationToken))
            return requestedId;

        var currentDepartmentId = await db.Users
            .Where(x => x.Id == currentUserId)
            .Select(x => x.DepartmentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!currentDepartmentId.HasValue || target.DepartmentId != currentDepartmentId)
            throw new UnauthorizedAccessException("Нельзя назначить задачу сотруднику другого подразделения.");

        return requestedId;
    }

    private async Task<bool> CanAccessAsync(
        TaskItem task,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();

        if (await currentUser.HasPermissionAsync(PermissionCodes.TasksViewAll, cancellationToken))
            return true;

        if (task.AssignedToId == userId || task.CreatedById == userId)
            return true;

        if (!await currentUser.HasPermissionAsync(PermissionCodes.TasksViewDepartment, cancellationToken))
            return false;

        var departmentId = await db.Users
            .Where(x => x.Id == userId)
            .Select(x => x.DepartmentId)
            .FirstOrDefaultAsync(cancellationToken);

        return departmentId.HasValue &&
               await db.Users.AnyAsync(
                   x => x.Id == task.AssignedToId &&
                        x.DepartmentId == departmentId.Value,
                   cancellationToken);
    }

    private async Task<TaskItem> LoadTaskAsync(int id, CancellationToken cancellationToken)
    {
        return await db.Tasks
            .AsNoTracking()
            .Include(x => x.CreatedBy)
            .Include(x => x.AssignedTo)
                .ThenInclude(x => x.Department)
            .Include(x => x.Comments)
                .ThenInclude(x => x.Author)
            .Include(x => x.History)
                .ThenInclude(x => x.ChangedBy)
            .FirstAsync(x => x.Id == id, cancellationToken);
    }

    private static TaskDto MapDetails(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            IsImportant = task.IsImportant,
            Progress = task.Progress,
            StartDate = task.StartDate,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CreatedById = task.CreatedById,
            CreatedByName = task.CreatedBy.DisplayName,
            AssignedToId = task.AssignedToId,
            AssignedToName = task.AssignedTo.DisplayName,
            AssignedToDepartmentName = task.AssignedTo.Department?.Name,
            CommentCount = task.Comments.Count,
            Comments = task.Comments
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new TaskCommentDto
                {
                    Id = x.Id,
                    AuthorId = x.AuthorId,
                    AuthorName = x.Author.DisplayName,
                    Text = x.Text,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToArray(),
            History = task.History
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new TaskHistoryDto
                {
                    Id = x.Id,
                    ChangedById = x.ChangedById,
                    ChangedByName = x.ChangedBy.DisplayName,
                    OldStatus = x.OldStatus,
                    NewStatus = x.NewStatus,
                    OldPriority = x.OldPriority,
                    NewPriority = x.NewPriority,
                    OldAssignedToId = x.OldAssignedToId,
                    NewAssignedToId = x.NewAssignedToId,
                    OldDueDate = x.OldDueDate,
                    NewDueDate = x.NewDueDate,
                    Comment = x.Comment,
                    CreatedAt = x.CreatedAt
                })
                .ToArray()
        };
    }

    private int RequireUserId()
        => currentUser.UserId
           ?? throw new UnauthorizedAccessException("Пользователь не авторизован.");

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название задачи не может быть пустым.");
        if (title.Trim().Length > 300)
            throw new ArgumentException("Название задачи не может быть длиннее 300 символов.");
    }

    private static void ValidateDates(DateTime? startDate, DateTime? dueDate)
    {
        if (startDate.HasValue && dueDate.HasValue && dueDate < startDate)
            throw new ArgumentException("Срок выполнения не может быть раньше даты начала.");
    }

    private static void ValidateEnum<T>(T value) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ArgumentException($"Недопустимое значение {typeof(T).Name}.");
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
