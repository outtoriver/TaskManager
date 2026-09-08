export type TaskStatus = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8;
export type TaskPriority = 0 | 1 | 2 | 3;

export interface TaskListItem {
  id: number;
  title: string;
  status: TaskStatus;
  priority: TaskPriority;
  isImportant: boolean;
  progress: number;
  startDate: string | null;
  dueDate: string | null;
  completedAt: string | null;
  createdAt: string;
  updatedAt: string;
  createdById: number;
  createdByName: string;
  assignedToId: number;
  assignedToName: string;
  assignedToDepartmentName: string | null;
  commentCount: number;
}

export interface TaskComment { id: number; authorId: number; authorName: string; text: string; createdAt: string; updatedAt: string | null; }
export interface TaskHistory { id: number; changedById: number; changedByName: string; oldStatus: TaskStatus | null; newStatus: TaskStatus | null; oldPriority: TaskPriority | null; newPriority: TaskPriority | null; oldAssignedToId: number | null; newAssignedToId: number | null; oldDueDate: string | null; newDueDate: string | null; comment: string | null; createdAt: string; }
export interface Task extends TaskListItem { description: string | null; comments: TaskComment[]; history: TaskHistory[]; }

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, { credentials: 'include', headers: { ...(init?.body ? { 'Content-Type': 'application/json' } : {}), ...(init?.headers ?? {}) }, ...init });
  if (!response.ok) {
    let message = `Ошибка запроса (${response.status}).`;
    try { const payload = await response.json() as { message?: string }; if (payload.message) message = payload.message; } catch { /* non-json */ }
    throw new Error(message);
  }
  if (response.status === 204) return undefined as T;
  return await response.json() as T;
}

export const tasksApi = {
  getAll: () => request<TaskListItem[]>('/api/tasks'),
  get: (id: number) => request<Task>(`/api/tasks/${id}`),
  create: (payload: { title: string; description?: string | null; priority: TaskPriority; isImportant: boolean; startDate?: string | null; dueDate?: string | null; assignedToId: number }) => request<Task>('/api/tasks', { method: 'POST', body: JSON.stringify(payload) }),
  update: (id: number, payload: { title: string; description?: string | null; priority: TaskPriority; isImportant: boolean; progress: number; startDate?: string | null; dueDate?: string | null; assignedToId: number }) => request<Task>(`/api/tasks/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
  remove: (id: number) => request<void>(`/api/tasks/${id}`, { method: 'DELETE' }),
  changeStatus: (id: number, status: TaskStatus, progress?: number, comment?: string) => request<Task>(`/api/tasks/${id}/status`, { method: 'POST', body: JSON.stringify({ status, progress, comment }) }),
  changePriority: (id: number, priority: TaskPriority, comment?: string) => request<Task>(`/api/tasks/${id}/priority`, { method: 'POST', body: JSON.stringify({ priority, comment }) }),
  comment: (id: number, text: string) => request<TaskComment>(`/api/tasks/${id}/comments`, { method: 'POST', body: JSON.stringify({ text }) }),
};
