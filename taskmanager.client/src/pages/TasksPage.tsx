import { useEffect, useMemo, useState } from 'react';
import { adminApi, type UserListItem } from '../api/adminApi';
import { useAuth } from '../auth/AuthContext';
import { AppPageShell } from '../components/AppPageShell';
import { tasksApi, type Task, type TaskListItem, type TaskPriority, type TaskStatus } from '../api/tasksApi';
import './TasksPage.css';

const columns: Array<{ value: TaskStatus; label: string; accent: string }> = [
  { value: 0, label: 'Новые', accent: 'blue' },
  { value: 1, label: 'Назначены', accent: 'indigo' },
  { value: 2, label: 'Приняты', accent: 'violet' },
  { value: 3, label: 'В работе', accent: 'cyan' },
  { value: 4, label: 'На проверке', accent: 'amber' },
  { value: 5, label: 'Завершены', accent: 'green' },
  { value: 6, label: 'На паузе', accent: 'orange' },
  { value: 7, label: 'Заблокированы', accent: 'red' },
  { value: 8, label: 'Отменены', accent: 'gray' },
];

const priorities: Array<{ value: TaskPriority; label: string }> = [
  { value: 0, label: 'Низкий' },
  { value: 1, label: 'Обычный' },
  { value: 2, label: 'Высокий' },
  { value: 3, label: 'Критический' },
];

const statusLabel = (value: TaskStatus) => columns.find(x => x.value === value)?.label ?? '—';
const priorityLabel = (value: TaskPriority) => priorities.find(x => x.value === value)?.label ?? '—';

export function TasksPage() {
  const { user, hasPermission } = useAuth();
  const [tasks, setTasks] = useState<TaskListItem[]>([]);
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [selected, setSelected] = useState<Task | null>(null);
  const [loading, setLoading] = useState(true);
  const [savingId, setSavingId] = useState<number | null>(null);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [showImportant, setShowImportant] = useState(false);
  const [showCreate, setShowCreate] = useState(false);

  const canCreate = hasPermission('Tasks.Create');
  const canEdit = hasPermission('Tasks.Edit');
  const canDelete = hasPermission('Tasks.Delete');
  const canAssign = hasPermission('Tasks.Assign');

  async function load() {
    setLoading(true);
    setError('');
    try {
      const [items, people] = await Promise.all([
        tasksApi.getAll(),
        hasPermission('Users.View') ? adminApi.getUsers() : Promise.resolve([] as UserListItem[]),
      ]);
      setTasks(items);
      setUsers(people.filter(x => x.isActive));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось загрузить задачи.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { void load(); }, []);

  const filtered = useMemo(() => {
    const query = search.trim().toLowerCase();
    return tasks.filter(task => {
      const haystack = `${task.title} ${task.assignedToName} ${task.createdByName}`.toLowerCase();
      return (!query || haystack.includes(query)) && (!showImportant || task.isImportant);
    });
  }, [tasks, search, showImportant]);

  async function openTask(id: number) {
    try {
      setSelected(await tasksApi.get(id));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось открыть задачу.');
    }
  }

  async function moveTask(task: TaskListItem, nextStatus: TaskStatus) {
    if (!canEdit || task.status === nextStatus || savingId === task.id) return;

    const previous = task.status;
    setSavingId(task.id);
    setError('');
    setTasks(current => current.map(x => x.id === task.id ? { ...x, status: nextStatus, progress: nextStatus === 5 ? 100 : x.progress } : x));

    try {
      const updated = await tasksApi.changeStatus(task.id, nextStatus, nextStatus === 5 ? 100 : task.progress);
      if (selected?.id === task.id) setSelected(updated);
      await load();
    } catch (e) {
      setTasks(current => current.map(x => x.id === task.id ? { ...x, status: previous } : x));
      setError(e instanceof Error ? e.message : 'Не удалось изменить статус задачи.');
    } finally {
      setSavingId(null);
    }
  }

  async function removeTask() {
    if (!selected) return;
    if (!window.confirm(`Удалить задачу «${selected.title}»?`)) return;
    try {
      await tasksApi.remove(selected.id);
      setSelected(null);
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось удалить задачу.');
    }
  }

  async function addComment(text: string) {
    if (!selected || !text.trim()) return;
    try {
      await tasksApi.comment(selected.id, text.trim());
      setSelected(await tasksApi.get(selected.id));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось добавить комментарий.');
    }
  }

  if (!user) return null;

  return (
    <AppPageShell
      title="Задачи"
      eyebrow="TASKMANAGER / WORKFLOW"
      subtitle="Перетаскивайте карточки между этапами. Изменение статуса сохраняется сразу."
    >
      <div className="tasks-toolbar">
        <div className="tasks-search-wrap">
          <span>⌕</span>
          <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Поиск задач, исполнителей..." />
        </div>
        <button type="button" className={showImportant ? 'tasks-filter active' : 'tasks-filter'} onClick={() => setShowImportant(x => !x)}>★ Важные</button>
        <button type="button" className="tasks-filter" onClick={() => void load()}>Обновить</button>
        {canCreate && <button type="button" className="tasks-primary" onClick={() => setShowCreate(true)}>＋ Новая задача</button>}
      </div>

      {error && <div className="tasks-alert">{error}</div>}

      <div className="tasks-board-meta">
        <div><strong>{filtered.length}</strong> задач</div>
        <div className="tasks-board-help">Перетащите карточку в нужную колонку</div>
      </div>

      {loading ? (
        <div className="tasks-loading">Загрузка задач…</div>
      ) : (
        <div className="tasks-board" aria-label="Канбан задач">
          {columns.map(column => {
            const items = filtered.filter(task => task.status === column.value);
            return (
              <section
                className={`task-column task-column-${column.accent}`}
                key={column.value}
                onDragOver={e => { if (canEdit) e.preventDefault(); }}
                onDrop={e => {
                  e.preventDefault();
                  const taskId = Number(e.dataTransfer.getData('text/task-id'));
                  const task = tasks.find(x => x.id === taskId);
                  if (task) void moveTask(task, column.value);
                }}
              >
                <div className="task-column-header">
                  <div>
                    <span className="task-column-dot" />
                    <h2>{column.label}</h2>
                  </div>
                  <span className="task-column-count">{items.length}</span>
                </div>
                <div className="task-column-body">
                  {items.map(task => (
                    <article
                      key={task.id}
                      className={`task-card ${selected?.id === task.id ? 'selected' : ''} ${savingId === task.id ? 'saving' : ''}`}
                      draggable={canEdit}
                      onDragStart={e => {
                        e.dataTransfer.effectAllowed = 'move';
                        e.dataTransfer.setData('text/task-id', String(task.id));
                      }}
                      onClick={() => void openTask(task.id)}
                    >
                      <div className="task-card-top">
                        {task.isImportant && <span className="task-card-star">★</span>}
                        <span className={`task-card-priority priority-${task.priority}`}>{priorityLabel(task.priority)}</span>
                        <span className="task-card-id">#{task.id}</span>
                      </div>
                      <h3>{task.title}</h3>
                      <div className="task-card-person">{task.assignedToName}</div>
                      {task.dueDate && <div className="task-card-due">Срок · {new Date(task.dueDate).toLocaleDateString('ru-RU')}</div>}
                      <div className="task-card-footer">
                        <div className="task-mini-progress"><span><i style={{ width: `${task.progress}%` }} /></span></div>
                        <span>{task.progress}%</span>
                      </div>
                    </article>
                  ))}
                  {items.length === 0 && <div className="task-column-empty">Перетащите задачу сюда</div>}
                </div>
              </section>
            );
          })}
        </div>
      )}

      {selected && (
        <div className="task-drawer-backdrop" onMouseDown={e => { if (e.target === e.currentTarget) setSelected(null); }}>
          <aside className="task-drawer">
            <div className="task-drawer-head">
              <div>
                <span className="task-drawer-eyebrow">TASK #{selected.id}</span>
                <h2>{selected.title}</h2>
              </div>
              <button type="button" className="task-close" onClick={() => setSelected(null)}>×</button>
            </div>
            <div className="task-drawer-tags">
              <span className={`task-status-chip status-${selected.status}`}>{statusLabel(selected.status)}</span>
              <span className={`task-priority-chip priority-${selected.priority}`}>{priorityLabel(selected.priority)}</span>
              {selected.isImportant && <span className="task-important-chip">★ Важная</span>}
            </div>
            {selected.description && <p className="task-drawer-description">{selected.description}</p>}
            <div className="task-drawer-grid">
              <div><span>Исполнитель</span><strong>{selected.assignedToName}</strong></div>
              <div><span>Создатель</span><strong>{selected.createdByName}</strong></div>
              <div><span>Начало</span><strong>{selected.startDate ? new Date(selected.startDate).toLocaleDateString('ru-RU') : '—'}</strong></div>
              <div><span>Срок</span><strong>{selected.dueDate ? new Date(selected.dueDate).toLocaleDateString('ru-RU') : '—'}</strong></div>
            </div>
            <div className="task-drawer-progress">
              <div><span>Прогресс</span><strong>{selected.progress}%</strong></div>
              <div className="task-progress-track"><i style={{ width: `${selected.progress}%` }} /></div>
            </div>
            {canEdit && <label className="task-drawer-field">Статус<select value={selected.status} onChange={async e => {
              const next = Number(e.target.value) as TaskStatus;
              const updated = await tasksApi.changeStatus(selected.id, next, next === 5 ? 100 : selected.progress);
              setSelected(updated);
              await load();
            }}>{columns.map(column => <option key={column.value} value={column.value}>{column.label}</option>)}</select></label>}
            <section className="task-drawer-section">
              <div className="task-section-title"><h3>Комментарии</h3><span>{selected.comments.length}</span></div>
              {selected.comments.length === 0 ? <p className="task-muted">Комментариев пока нет.</p> : selected.comments.map(comment => (
                <article className="task-comment" key={comment.id}><strong>{comment.authorName}</strong><time>{new Date(comment.createdAt).toLocaleString('ru-RU')}</time><p>{comment.text}</p></article>
              ))}
              <CommentBox onSubmit={addComment} />
            </section>
            <section className="task-drawer-section">
              <div className="task-section-title"><h3>История</h3><span>{selected.history.length}</span></div>
              {selected.history.length === 0 ? <p className="task-muted">Изменений ещё нет.</p> : selected.history.map(history => (
                <article className="task-history" key={history.id}><strong>{history.changedByName}</strong><time>{new Date(history.createdAt).toLocaleString('ru-RU')}</time><p>{history.comment ?? `Изменение статуса: ${statusLabel(history.newStatus as TaskStatus)}`}</p></article>
              ))}
            </section>
            {canDelete && <button type="button" className="task-danger" onClick={() => void removeTask()}>Удалить задачу</button>}
            {canAssign && <span className="task-drawer-note">Назначение исполнителя задаётся при создании/редактировании задачи.</span>}
          </aside>
        </div>
      )}

      {showCreate && <TaskModal users={users} currentUserId={user.id} canAssign={canAssign} onClose={() => setShowCreate(false)} onSaved={async () => { setShowCreate(false); await load(); }} />}
    </AppPageShell>
  );
}

function CommentBox({ onSubmit }: { onSubmit: (text: string) => Promise<void> }) {
  const [text, setText] = useState('');
  return (
    <div className="task-comment-box">
      <textarea value={text} onChange={e => setText(e.target.value)} placeholder="Добавить комментарий…" />
      <button type="button" onClick={async () => { if (!text.trim()) return; await onSubmit(text); setText(''); }}>Добавить</button>
    </div>
  );
}

function TaskModal({ users, currentUserId, canAssign, onClose, onSaved }: { users: UserListItem[]; currentUserId: number; canAssign: boolean; onClose: () => void; onSaved: () => Promise<void> }) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState<TaskPriority>(1);
  const [important, setImportant] = useState(false);
  const [assignedToId, setAssignedToId] = useState(canAssign ? (users[0]?.id ?? currentUserId) : currentUserId);
  const [dueDate, setDueDate] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  async function submit() {
    if (!title.trim()) { setError('Введите название задачи.'); return; }
    try {
      setSaving(true);
      setError('');
      await tasksApi.create({
        title: title.trim(),
        description: description.trim() || null,
        priority,
        isImportant: important,
        startDate: null,
        dueDate: dueDate ? new Date(`${dueDate}T23:59:59`).toISOString() : null,
        assignedToId,
      });
      await onSaved();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось создать задачу.');
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="task-modal-backdrop">
      <div className="task-modal">
        <div className="task-modal-head"><div><span className="task-drawer-eyebrow">NEW TASK</span><h2>Новая задача</h2></div><button type="button" className="task-close" onClick={onClose}>×</button></div>
        {error && <div className="tasks-alert">{error}</div>}
        <label>Название<input value={title} onChange={e => setTitle(e.target.value)} autoFocus /></label>
        <label>Описание<textarea value={description} onChange={e => setDescription(e.target.value)} /></label>
        <div className="task-form-row">
          <label>Приоритет<select value={priority} onChange={e => setPriority(Number(e.target.value) as TaskPriority)}>{priorities.map(x => <option key={x.value} value={x.value}>{x.label}</option>)}</select></label>
          <label>Срок<input type="date" value={dueDate} onChange={e => setDueDate(e.target.value)} /></label>
        </div>
        {canAssign && <label>Исполнитель<select value={assignedToId} onChange={e => setAssignedToId(Number(e.target.value))}>{users.map(x => <option key={x.id} value={x.id}>{x.displayName}{x.departmentName ? ` · ${x.departmentName}` : ''}</option>)}</select></label>}
        <label className="task-check"><input type="checkbox" checked={important} onChange={e => setImportant(e.target.checked)} /> Отметить как важную</label>
        <div className="task-modal-actions"><button type="button" className="tasks-filter" onClick={onClose}>Отмена</button><button type="button" className="tasks-primary" disabled={saving} onClick={() => void submit()}>{saving ? 'Сохранение…' : 'Создать задачу'}</button></div>
      </div>
    </div>
  );
}
