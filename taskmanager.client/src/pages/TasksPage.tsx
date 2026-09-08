import { useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { adminApi, type UserListItem } from '../api/adminApi';
import { tasksApi, type Task, type TaskListItem, type TaskPriority, type TaskStatus } from '../api/tasksApi';
import './TasksPage.css';

const statuses: Array<{ value: TaskStatus; label: string }> = [
  { value: 0, label: 'Новые' }, { value: 1, label: 'Назначены' }, { value: 2, label: 'Приняты' },
  { value: 3, label: 'В работе' }, { value: 4, label: 'На проверке' }, { value: 5, label: 'Завершены' },
  { value: 6, label: 'На паузе' }, { value: 7, label: 'Заблокированы' }, { value: 8, label: 'Отменены' },
];
const priorities: Array<{ value: TaskPriority; label: string }> = [
  { value: 0, label: 'Низкий' }, { value: 1, label: 'Обычный' }, { value: 2, label: 'Высокий' }, { value: 3, label: 'Критический' },
];

const statusLabel = (value: TaskStatus) => statuses.find(x => x.value === value)?.label ?? '—';
const priorityLabel = (value: TaskPriority) => priorities.find(x => x.value === value)?.label ?? '—';
const formatDate = (value: string | null) => value ? new Date(value).toLocaleDateString('ru-RU') : 'Без срока';

export function TasksPage() {
  const { user, hasPermission } = useAuth();
  const [tasks, setTasks] = useState<TaskListItem[]>([]);
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [selected, setSelected] = useState<Task | null>(null);
  const [view, setView] = useState<'kanban' | 'list'>('kanban');
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | TaskStatus>('all');
  const [importantOnly, setImportantOnly] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [draggedId, setDraggedId] = useState<number | null>(null);

  const canCreate = hasPermission('Tasks.Create');
  const canEdit = hasPermission('Tasks.Edit');
  const canDelete = hasPermission('Tasks.Delete');
  const canAssign = hasPermission('Tasks.Assign');
  const canApprove = hasPermission('Tasks.Approve');
  const canSeePeople = hasPermission('Users.View');
  const canSeeAllTasks = hasPermission('Tasks.ViewAll');

  async function load() {
    setLoading(true); setError('');
    try {
      const [items, people] = await Promise.all([
        tasksApi.getAll(),
        canSeePeople ? adminApi.getUsers() : Promise.resolve([] as UserListItem[]),
      ]);
      setTasks(items);
      setUsers(people.filter(x => x.isActive && (canSeeAllTasks || x.id === Number(user.id) || x.managerId === Number(user.id))));
    } catch (e) { setError(e instanceof Error ? e.message : 'Не удалось загрузить задачи.'); }
    finally { setLoading(false); }
  }

  useEffect(() => { void load(); }, []);

  const filtered = useMemo(() => tasks.filter(t => {
    const haystack = `${t.title} ${t.assignedToName} ${t.createdByName}`.toLowerCase();
    return (!search || haystack.includes(search.toLowerCase())) &&
      (statusFilter === 'all' || t.status === statusFilter) &&
      (!importantOnly || t.isImportant);
  }), [tasks, search, statusFilter, importantOnly]);

  async function openTask(id: number) {
    try { setSelected(await tasksApi.get(id)); }
    catch (e) { setError(e instanceof Error ? e.message : 'Не удалось открыть задачу.'); }
  }

  async function moveTask(id: number, next: TaskStatus) {
    if (!canEdit) return;
    try {
      setSaving(true); setError('');
      await tasksApi.changeStatus(id, next, next === 5 ? 100 : undefined);
      if (selected?.id === id) setSelected(await tasksApi.get(id));
      await load();
    } catch (e) { setError(e instanceof Error ? e.message : 'Не удалось изменить статус.'); }
    finally { setSaving(false); setDraggedId(null); }
  }

  async function removeTask() {
    if (!selected || !canDelete) return;
    if (!window.confirm(`Удалить задачу «${selected.title}»?`)) return;
    try { setSaving(true); await tasksApi.remove(selected.id); setSelected(null); await load(); }
    catch (e) { setError(e instanceof Error ? e.message : 'Не удалось удалить задачу.'); }
    finally { setSaving(false); }
  }

  async function addComment(text: string) {
    if (!selected || !text.trim()) return;
    try { await tasksApi.comment(selected.id, text.trim()); setSelected(await tasksApi.get(selected.id)); }
    catch (e) { setError(e instanceof Error ? e.message : 'Не удалось добавить комментарий.'); }
  }

  if (!user) return null;

  return <div className="tasks-page-full">
    <header className="tasks-header">
      <div className="tasks-title-area">
        <button className="tasks-back" type="button" onClick={() => window.history.length > 1 ? window.history.back() : (window.location.hash = '#dashboard')}>← Назад</button>
        <div><span className="eyebrow">TASKMANAGER / WORKFLOW</span><h1>Задачи</h1><p>Рабочий контур задач с назначением, приоритетами и контролируемым процессом выполнения.</p></div>
      </div>
      {canCreate && <button className="task-primary-button" type="button" onClick={() => setShowCreate(true)}>＋ Новая задача</button>}
    </header>

    <div className="tasks-toolbar">
      <input value={search} onChange={e => setSearch(e.target.value)} placeholder="Поиск задач, исполнителей..." />
      <select value={statusFilter} onChange={e => setStatusFilter(e.target.value === 'all' ? 'all' : Number(e.target.value) as TaskStatus)}><option value="all">Все статусы</option>{statuses.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}</select>
      <button className={importantOnly ? 'filter-button active' : 'filter-button'} onClick={() => setImportantOnly(x => !x)}>★ Важные</button>
      <div className="view-switch"><button className={view === 'kanban' ? 'active' : ''} onClick={() => setView('kanban')}>Kanban</button><button className={view === 'list' ? 'active' : ''} onClick={() => setView('list')}>Список</button></div>
      <button className="filter-button" onClick={() => void load()} disabled={loading}>Обновить</button>
    </div>

    {error && <div className="task-error">{error}</div>}
    <div className="tasks-summary"><strong>{filtered.length}</strong><span>задач в текущей выборке</span>{saving && <span className="saving-label">Сохранение…</span>}</div>

    {loading ? <div className="task-empty">Загрузка...</div> : filtered.length === 0 ? <div className="task-empty"><strong>Задач пока нет</strong><span>Создайте первую задачу или измените фильтры.</span></div> : view === 'kanban' ? (
      <div className="kanban-board">
        {statuses.map(column => {
          const columnTasks = filtered.filter(t => t.status === column.value);
          return <section key={column.value} className={`kanban-column status-column-${column.value}`} onDragOver={e => e.preventDefault()} onDrop={e => { e.preventDefault(); if (draggedId !== null) void moveTask(draggedId, column.value); }}>
            <header className="kanban-column-header"><div><span className="column-index">0{column.value + 1}</span><h2>{column.label}</h2></div><span>{columnTasks.length}</span></header>
            <div className="kanban-cards">
              {columnTasks.map(task => <article key={task.id} className="kanban-card" draggable={canEdit} onDragStart={() => setDraggedId(task.id)} onDragEnd={() => setDraggedId(null)} onClick={() => void openTask(task.id)}>
                <div className="kanban-card-top"><span className={`task-priority priority-${task.priority}`}>{priorityLabel(task.priority)}</span>{task.isImportant && <span className="important-star">★</span>}</div>
                <h3>{task.title}</h3>
                <p>#{task.id} · {task.assignedToName}</p>
                <div className="kanban-card-bottom"><span>{formatDate(task.dueDate)}</span><strong>{task.progress}%</strong></div>
                <div className="task-progress"><span><i style={{ width: `${task.progress}%` }} /></span></div>
              </article>)}
              {columnTasks.length === 0 && <div className="kanban-empty">Перетащите задачу сюда</div>}
            </div>
          </section>;
        })}
      </div>
    ) : (
      <div className="task-table-wrap"><table className="task-table"><thead><tr><th>Задача</th><th>Статус</th><th>Приоритет</th><th>Исполнитель</th><th>Срок</th><th>Прогресс</th></tr></thead><tbody>{filtered.map(task => <tr key={task.id} onClick={() => void openTask(task.id)}><td><strong>{task.isImportant ? '★ ' : ''}{task.title}</strong><small>#{task.id}</small></td><td><span className={`task-status status-${task.status}`}>{statusLabel(task.status)}</span></td><td><span className={`task-priority priority-${task.priority}`}>{priorityLabel(task.priority)}</span></td><td>{task.assignedToName}</td><td>{formatDate(task.dueDate)}</td><td><div className="task-progress"><span><i style={{ width: `${task.progress}%` }} /></span><em>{task.progress}%</em></div></td></tr>)}</tbody></table></div>
    )}

    {selected && <aside className="task-detail-panel">
      <div className="detail-top"><div><span className="eyebrow">TASK #{selected.id}</span><h2>{selected.title}</h2></div><button className="icon-close" type="button" onClick={() => setSelected(null)}>×</button></div>
      <div className="detail-tags"><span className={`task-status status-${selected.status}`}>{statusLabel(selected.status)}</span><span className={`task-priority priority-${selected.priority}`}>{priorityLabel(selected.priority)}</span>{selected.isImportant && <span className="important-tag">★ Важная</span>}</div>
      {selected.description && <p className="detail-description">{selected.description}</p>}
      <div className="detail-grid"><div><span>Исполнитель</span><strong>{selected.assignedToName}</strong></div><div><span>Создатель</span><strong>{selected.createdByName}</strong></div><div><span>Начало</span><strong>{formatDate(selected.startDate)}</strong></div><div><span>Срок</span><strong>{formatDate(selected.dueDate)}</strong></div></div>
      <div className="detail-progress"><div><span>Прогресс</span><strong>{selected.progress}%</strong></div><div className="big-progress"><span><i style={{ width: `${selected.progress}%` }} /></span></div></div>
      {canEdit && <div className="detail-actions"><select value={selected.status} onChange={e => void moveTask(selected.id, Number(e.target.value) as TaskStatus)} disabled={saving || (!canApprove && (Number(e.target.value) === 5 || Number(e.target.value) === 8))}>{statuses.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}</select></div>}
      <section className="detail-section"><div className="section-title"><h3>Комментарии</h3><span>{selected.comments.length}</span></div>{selected.comments.length ? <div className="comment-list">{selected.comments.map(c => <article key={c.id}><strong>{c.authorName}</strong><time>{new Date(c.createdAt).toLocaleString('ru-RU')}</time><p>{c.text}</p></article>)}</div> : <p className="muted">Комментариев пока нет.</p>}<CommentBox onSubmit={addComment} /></section>
      <section className="detail-section"><div className="section-title"><h3>История</h3><span>{selected.history.length}</span></div>{selected.history.length ? <div className="history-list">{selected.history.map(h => <article key={h.id}><strong>{h.changedByName}</strong><time>{new Date(h.createdAt).toLocaleString('ru-RU')}</time><p>{h.comment ?? `Статус: ${statusLabel(h.newStatus as TaskStatus)}`}</p></article>)}</div> : <p className="muted">Изменений ещё нет.</p>}</section>
      {canDelete && <button className="danger-button" type="button" onClick={() => void removeTask()}>Удалить задачу</button>}
    </aside>}

    {showCreate && <TaskModal currentUserId={Number(user.id)} users={users} canAssign={canAssign} onClose={() => setShowCreate(false)} onSaved={async () => { setShowCreate(false); await load(); }} />}
  </div>;
}

function CommentBox({ onSubmit }: { onSubmit: (text: string) => Promise<void> }) { const [text,setText]=useState(''); return <div className="comment-box"><textarea value={text} onChange={e=>setText(e.target.value)} placeholder="Добавить комментарий..."/><button onClick={async()=>{if(!text.trim())return;await onSubmit(text.trim());setText('')}}>Добавить</button></div>; }

function TaskModal({ currentUserId, users, canAssign, onClose, onSaved }: { currentUserId:number; users:UserListItem[]; canAssign:boolean; onClose:()=>void; onSaved:()=>Promise<void> }) {
  const [title,setTitle]=useState(''); const [description,setDescription]=useState(''); const [priority,setPriority]=useState<TaskPriority>(1); const [important,setImportant]=useState(false); const [assignedToId,setAssignedToId]=useState(currentUserId); const [dueDate,setDueDate]=useState(''); const [busy,setBusy]=useState(false); const [error,setError]=useState('');
  useEffect(()=>{ if(canAssign && users.length && assignedToId===currentUserId) setAssignedToId(currentUserId); },[canAssign,users,currentUserId,assignedToId]);
  async function submit(){ if(!title.trim()){setError('Введите название задачи.');return;} try{setBusy(true);setError('');await tasksApi.create({title:title.trim(),description:description.trim()||null,priority,isImportant:important,startDate:null,dueDate:dueDate?new Date(`${dueDate}T23:59:59`).toISOString():null,assignedToId});await onSaved();}catch(e){setError(e instanceof Error?e.message:'Не удалось создать задачу.');}finally{setBusy(false)} }
  return <div className="modal-backdrop"><div className="task-modal"><div className="modal-header"><div><span className="eyebrow">NEW TASK</span><h2>Новая задача</h2></div><button className="icon-close" onClick={onClose}>×</button></div>{error&&<div className="task-error">{error}</div>}<label>Название<input value={title} onChange={e=>setTitle(e.target.value)} autoFocus/></label><label>Описание<textarea value={description} onChange={e=>setDescription(e.target.value)}/></label><div className="form-row"><label>Приоритет<select value={priority} onChange={e=>setPriority(Number(e.target.value) as TaskPriority)}>{priorities.map(p=><option key={p.value} value={p.value}>{p.label}</option>)}</select></label><label>Срок<input type="date" value={dueDate} onChange={e=>setDueDate(e.target.value)}/></label></div><label>{canAssign?'Исполнитель':'Исполнитель'}<select value={assignedToId} onChange={e=>setAssignedToId(Number(e.target.value))} disabled={!canAssign}>{canAssign && users.length ? users.map(u=><option key={u.id} value={u.id}>{u.displayName}{u.departmentName?` · ${u.departmentName}`:''}</option>) : <option value={currentUserId}>Я</option>}</select></label><label className="check-row"><input type="checkbox" checked={important} onChange={e=>setImportant(e.target.checked)}/> Отметить как важную</label><div className="modal-actions"><button className="filter-button" onClick={onClose}>Отмена</button><button className="task-primary-button" disabled={busy} onClick={()=>void submit()}>{busy?'Сохранение...':'Создать задачу'}</button></div></div></div>;
}
