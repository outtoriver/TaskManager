import { useEffect, useMemo, useState } from 'react';
import { tasksApi, type Task, type TaskListItem, type TaskPriority, type TaskStatus } from '../api/tasksApi';
import { adminApi, type UserListItem } from '../api/adminApi';
import { useAuth } from '../auth/AuthContext';
import './TasksPage.css';

const statuses: Array<{ value: TaskStatus; label: string }> = [
  { value: 0, label: 'Новые' }, { value: 1, label: 'Назначены' }, { value: 2, label: 'Приняты' }, { value: 3, label: 'В работе' },
  { value: 4, label: 'На проверке' }, { value: 5, label: 'Завершены' }, { value: 6, label: 'На паузе' }, { value: 7, label: 'Заблокированы' }, { value: 8, label: 'Отменены' },
];
const priorities: Array<{ value: TaskPriority; label: string }> = [
  { value: 0, label: 'Низкий' }, { value: 1, label: 'Обычный' }, { value: 2, label: 'Высокий' }, { value: 3, label: 'Критический' },
];
const statusLabel = (value: TaskStatus) => statuses.find(x => x.value === value)?.label ?? '—';
const priorityLabel = (value: TaskPriority) => priorities.find(x => x.value === value)?.label ?? '—';
const priorityClass = (value: TaskPriority) => `task-priority-${value}`;

export function TasksPage() {
  const { user, hasPermission } = useAuth();
  const [tasks, setTasks] = useState<TaskListItem[]>([]);
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [selected, setSelected] = useState<Task | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<'all' | TaskStatus>('all');
  const [showImportant, setShowImportant] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [saving, setSaving] = useState(false);

  const canCreate = hasPermission('Tasks.Create');
  const canEdit = hasPermission('Tasks.Edit');
  const canDelete = hasPermission('Tasks.Delete');
  const canAssign = hasPermission('Tasks.Assign');

  const load = async () => {
    setLoading(true);
    setError('');
    try {
      const [items, people] = await Promise.all([
        tasksApi.getAll(),
        hasPermission('Users.View') ? adminApi.getUsers() : Promise.resolve([] as UserListItem[]),
      ]);
      setTasks(items);
      setUsers(people.filter(x => x.isActive));
    } catch (e) { setError(e instanceof Error ? e.message : 'Не удалось загрузить задачи.'); }
    finally { setLoading(false); }
  };

  useEffect(() => { void load(); }, []);

  const filtered = useMemo(() => tasks.filter(task => {
    const text = `${task.title} ${task.assignedToName} ${task.createdByName}`.toLowerCase();
    return (!search || text.includes(search.toLowerCase())) && (status === 'all' || task.status === status) && (!showImportant || task.isImportant);
  }), [tasks, search, status, showImportant]);

  const openTask = async (id: number) => {
    try { setSelected(await tasksApi.get(id)); } catch (e) { setError(e instanceof Error ? e.message : 'Не удалось открыть задачу.'); }
  };

  const removeTask = async () => {
    if (!selected) return;
    if (!window.confirm(`Удалить задачу «${selected.title}»?`)) return;
    try { await tasksApi.remove(selected.id); setSelected(null); await load(); } catch (e) { setError(e instanceof Error ? e.message : 'Не удалось удалить задачу.'); }
  };

  const updateStatus = async (next: TaskStatus) => {
    if (!selected) return;
    try { setSaving(true); setSelected(await tasksApi.changeStatus(selected.id, next, next === 5 ? 100 : selected.progress)); await load(); }
    catch (e) { setError(e instanceof Error ? e.message : 'Не удалось изменить статус.'); }
    finally { setSaving(false); }
  };

  const addComment = async (text: string) => {
    if (!selected || !text.trim()) return;
    try { await tasksApi.comment(selected.id, text.trim()); setSelected(await tasksApi.get(selected.id)); }
    catch (e) { setError(e instanceof Error ? e.message : 'Не удалось добавить комментарий.'); }
  };

  if (!user) return null;

  return <div className="tasks-page">
    <div className="tasks-main">
      <header className="tasks-header">
        <div><span className="eyebrow">TASKMANAGER / WORKFLOW</span><h1>Задачи</h1><p>Рабочий список задач с приоритетами, исполнителями, сроками и историей изменений.</p></div>
        {canCreate && <button className="task-primary-button" type="button" onClick={() => setShowCreate(true)}>＋ Новая задача</button>}
      </header>

      <div className="tasks-toolbar">
        <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Поиск задач, исполнителей..." />
        <select value={status} onChange={e => setStatus(e.target.value === 'all' ? 'all' : Number(e.target.value) as TaskStatus)}><option value="all">Все статусы</option>{statuses.map(x => <option key={x.value} value={x.value}>{x.label}</option>)}</select>
        <button className={showImportant ? 'filter-button active' : 'filter-button'} type="button" onClick={() => setShowImportant(x => !x)}>★ Важные</button>
        <button className="filter-button" type="button" onClick={() => void load()}>Обновить</button>
      </div>

      {error && <div className="task-error">{error}</div>}
      <div className="tasks-summary"><strong>{filtered.length}</strong><span>задач в текущей выборке</span></div>

      {loading ? <div className="task-empty">Загрузка...</div> : filtered.length === 0 ? <div className="task-empty"><strong>Задач пока нет</strong><span>Создай первую задачу или измени фильтры.</span></div> :
        <div className="task-table-wrap"><table className="task-table"><thead><tr><th>Задача</th><th>Статус</th><th>Приоритет</th><th>Исполнитель</th><th>Срок</th><th>Прогресс</th></tr></thead><tbody>{filtered.map(task => <tr key={task.id} className={selected?.id === task.id ? 'selected' : ''} onClick={() => void openTask(task.id)}>
          <td><div className="task-title-cell">{task.isImportant && <span className="important-star">★</span>}<div><strong>{task.title}</strong><small>#{task.id} · создана {new Date(task.createdAt).toLocaleDateString('ru-RU')}</small></div></div></td>
          <td><span className={`task-status status-${task.status}`}>{statusLabel(task.status)}</span></td>
          <td><span className={`task-priority ${priorityClass(task.priority)}`}>{priorityLabel(task.priority)}</span></td>
          <td>{task.assignedToName}</td>
          <td>{task.dueDate ? new Date(task.dueDate).toLocaleDateString('ru-RU') : '—'}</td>
          <td><div className="task-progress"><span><i style={{ width: `${task.progress}%` }} /></span><em>{task.progress}%</em></div></td>
        </tr>)}</tbody></table></div>}
    </div>

    {selected && <aside className="task-detail-panel">
      <div className="detail-top"><div><span className="eyebrow">TASK #{selected.id}</span><h2>{selected.title}</h2></div><button className="icon-close" onClick={() => setSelected(null)} type="button">×</button></div>
      <div className="detail-tags"><span className={`task-status status-${selected.status}`}>{statusLabel(selected.status)}</span><span className={`task-priority ${priorityClass(selected.priority)}`}>{priorityLabel(selected.priority)}</span>{selected.isImportant && <span className="important-tag">★ Важная</span>}</div>
      {selected.description && <p className="detail-description">{selected.description}</p>}
      <div className="detail-grid"><div><span>Исполнитель</span><strong>{selected.assignedToName}</strong></div><div><span>Создатель</span><strong>{selected.createdByName}</strong></div><div><span>Начало</span><strong>{selected.startDate ? new Date(selected.startDate).toLocaleDateString('ru-RU') : '—'}</strong></div><div><span>Срок</span><strong>{selected.dueDate ? new Date(selected.dueDate).toLocaleDateString('ru-RU') : '—'}</strong></div></div>
      <div className="detail-progress"><div><span>Прогресс</span><strong>{selected.progress}%</strong></div><div className="big-progress"><span><i style={{ width: `${selected.progress}%` }} /></span></div></div>
      {canEdit && <div className="detail-actions"><select value={selected.status} onChange={e => void updateStatus(Number(e.target.value) as TaskStatus)} disabled={saving}>{statuses.map(x => <option key={x.value} value={x.value}>{x.label}</option>)}</select></div>}
      <section className="detail-section"><div className="section-title"><h3>Комментарии</h3><span>{selected.comments.length}</span></div>{selected.comments.length === 0 ? <p className="muted">Комментариев пока нет.</p> : <div className="comment-list">{selected.comments.map(c => <article key={c.id}><strong>{c.authorName}</strong><time>{new Date(c.createdAt).toLocaleString('ru-RU')}</time><p>{c.text}</p></article>)}</div>}<CommentBox onSubmit={addComment} /></section>
      <section className="detail-section"><div className="section-title"><h3>История</h3><span>{selected.history.length}</span></div>{selected.history.length === 0 ? <p className="muted">Изменений ещё нет.</p> : <div className="history-list">{selected.history.map(h => <article key={h.id}><strong>{h.changedByName}</strong><time>{new Date(h.createdAt).toLocaleString('ru-RU')}</time><p>{h.comment ?? historyText(h)}</p></article>)}</div>}</section>
      {canDelete && <button className="danger-button" type="button" onClick={() => void removeTask()}>Удалить задачу</button>}
      {canAssign && <span className="detail-note">Назначение исполнителя доступно при создании/редактировании задачи.</span>}
    </aside>}

    {showCreate && <TaskModal users={users} currentUserId={Number(user.id)} canAssign={canAssign} onClose={() => setShowCreate(false)} onSaved={async () => { setShowCreate(false); await load(); }} />}
  </div>;
}

function CommentBox({ onSubmit }: { onSubmit: (text: string) => Promise<void> }) {
  const [text, setText] = useState('');
  const submit = async () => { if (!text.trim()) return; await onSubmit(text); setText(''); };
  return <div className="comment-box"><textarea value={text} onChange={e => setText(e.target.value)} placeholder="Добавить комментарий..." /><button type="button" onClick={() => void submit()}>Добавить</button></div>;
}

function TaskModal({ users, currentUserId, canAssign, onClose, onSaved }: { users: UserListItem[]; currentUserId: number; canAssign: boolean; onClose: () => void; onSaved: () => Promise<void> }) {
  const [title, setTitle] = useState(''); const [description, setDescription] = useState(''); const [priority, setPriority] = useState<TaskPriority>(1); const [important, setImportant] = useState(false); const [assignedToId, setAssignedToId] = useState(canAssign ? (users[0]?.id ?? currentUserId) : currentUserId); const [dueDate, setDueDate] = useState(''); const [saving, setSaving] = useState(false); const [error, setError] = useState('');
  const submit = async () => { if (!title.trim()) { setError('Введите название задачи.'); return; } try { setSaving(true); setError(''); await tasksApi.create({ title: title.trim(), description: description.trim() || null, priority, isImportant: important, startDate: null, dueDate: dueDate ? new Date(`${dueDate}T23:59:59`).toISOString() : null, assignedToId }); await onSaved(); } catch (e) { setError(e instanceof Error ? e.message : 'Не удалось создать задачу.'); } finally { setSaving(false); } };
  return <div className="modal-backdrop"><div className="task-modal"><div className="modal-header"><div><span className="eyebrow">NEW TASK</span><h2>Новая задача</h2></div><button className="icon-close" onClick={onClose} type="button">×</button></div>{error && <div className="task-error">{error}</div>}<label>Название<input value={title} onChange={e => setTitle(e.target.value)} autoFocus /></label><label>Описание<textarea value={description} onChange={e => setDescription(e.target.value)} /></label><div className="form-row"><label>Приоритет<select value={priority} onChange={e => setPriority(Number(e.target.value) as TaskPriority)}>{priorities.map(x => <option key={x.value} value={x.value}>{x.label}</option>)}</select></label><label>Срок<input type="date" value={dueDate} onChange={e => setDueDate(e.target.value)} /></label></div>{canAssign && <label>Исполнитель<select value={assignedToId} onChange={e => setAssignedToId(Number(e.target.value))}>{users.map(x => <option key={x.id} value={x.id}>{x.displayName}{x.departmentName ? ` · ${x.departmentName}` : ''}</option>)}</select></label>}<label className="check-row"><input type="checkbox" checked={important} onChange={e => setImportant(e.target.checked)} /> Отметить как важную</label><div className="modal-actions"><button type="button" className="filter-button" onClick={onClose}>Отмена</button><button type="button" className="task-primary-button" disabled={saving} onClick={() => void submit()}>{saving ? 'Сохранение...' : 'Создать задачу'}</button></div></div></div>;
}

function historyText(h: Task['history'][number]) { if (h.oldStatus !== h.newStatus) return `Статус: ${statusLabel(h.newStatus as TaskStatus)}`; if (h.oldPriority !== h.newPriority) return `Приоритет: ${priorityLabel(h.newPriority as TaskPriority)}`; if (h.oldAssignedToId !== h.newAssignedToId) return 'Изменён исполнитель'; if (h.oldDueDate !== h.newDueDate) return 'Изменён срок'; return 'Изменена задача'; }
