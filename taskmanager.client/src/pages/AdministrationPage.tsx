import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useAuth } from '../auth/AuthContext';
import { adminApi, type Department, type Position, type Role, type UserDetails, type UserListItem } from '../api/adminApi';

const permissions = {
  usersView: 'Users.View',
  usersCreate: 'Users.Create',
  usersEdit: 'Users.Edit',
  usersDelete: 'Users.Delete',
  departmentsView: 'Departments.View',
  departmentsManage: 'Departments.Manage',
  positionsView: 'Positions.View',
  positionsManage: 'Positions.Manage',
  rolesView: 'Roles.View',
  rolesManage: 'Roles.Manage',
} as const;

type Tab = 'users' | 'departments' | 'positions';

interface Notice { kind: 'success' | 'error'; text: string }

export function AdministrationPage() {
  const { hasPermission } = useAuth();
  const allowedUsers = hasPermission(permissions.usersView);
  const allowedDepartments = hasPermission(permissions.departmentsView);
  const allowedPositions = hasPermission(permissions.positionsView);
  const firstTab: Tab = allowedUsers ? 'users' : allowedDepartments ? 'departments' : 'positions';
  const [tab, setTab] = useState<Tab>(firstTab);
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [positions, setPositions] = useState<Position[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [notice, setNotice] = useState<Notice | null>(null);
  const [busy, setBusy] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserDetails | null>(null);
  const [userEditor, setUserEditor] = useState<UserEditorState | null>(null);
  const [selectedDepartment, setSelectedDepartment] = useState<Department | null>(null);
  const [selectedPosition, setSelectedPosition] = useState<Position | null>(null);
  const [passwordTarget, setPasswordTarget] = useState<UserListItem | null>(null);

  const canManageUsers = hasPermission(permissions.usersCreate) || hasPermission(permissions.usersEdit) || hasPermission(permissions.usersDelete);
  const canManageDepartments = hasPermission(permissions.departmentsManage);
  const canManagePositions = hasPermission(permissions.positionsManage);

  async function load() {
    setBusy(true);
    setNotice(null);
    try {
      const jobs: Promise<unknown>[] = [];
      if (allowedUsers) jobs.push(adminApi.getUsers().then(setUsers));
      if (allowedDepartments) jobs.push(adminApi.getDepartments().then(setDepartments));
      if (allowedPositions) jobs.push(adminApi.getPositions().then(setPositions));
      if (hasPermission(permissions.rolesView)) jobs.push(adminApi.getRoles().then(setRoles));
      await Promise.all(jobs);
    } catch (error) {
      setNotice({ kind: 'error', text: error instanceof Error ? error.message : 'Не удалось загрузить административные данные.' });
    } finally {
      setBusy(false);
    }
  }

  useEffect(() => { void load(); }, []);

  async function saveUser(payload: UserEditorState) {
    try {
      setBusy(true);
      if (payload.id) {
        await adminApi.updateUser(payload.id, {
          displayName: payload.displayName,
          email: payload.email || null,
          phone: payload.phone || null,
          departmentId: payload.departmentId || null,
          positionId: payload.positionId || null,
          managerId: payload.managerId || null,
          isActive: payload.isActive,
        });
      } else {
        await adminApi.createUser({
          login: payload.login.trim(),
          displayName: payload.displayName.trim(),
          email: payload.email || null,
          phone: payload.phone || null,
          departmentId: payload.departmentId || null,
          positionId: payload.positionId || null,
          managerId: payload.managerId || null,
          isActive: payload.isActive,
        });
      }
      setUserEditor(null);
      setNotice({ kind: 'success', text: payload.id ? 'Пользователь обновлён.' : 'Пользователь создан.' });
      await load();
    } catch (error) {
      setNotice({ kind: 'error', text: error instanceof Error ? error.message : 'Не удалось сохранить пользователя.' });
    } finally { setBusy(false); }
  }

  async function openUser(id: number) {
    try { setSelectedUser(await adminApi.getUser(id)); }
    catch (error) { setNotice({ kind: 'error', text: error instanceof Error ? error.message : 'Не удалось открыть пользователя.' }); }
  }

  async function removeUser(user: UserListItem) {
    if (!window.confirm(`Деактивировать пользователя «${user.displayName}»?`)) return;
    try { await adminApi.deleteUser(user.id); setNotice({ kind: 'success', text: 'Пользователь деактивирован.' }); await load(); }
    catch (error) { setNotice({ kind: 'error', text: error instanceof Error ? error.message : 'Не удалось деактивировать пользователя.' }); }
  }

  async function saveDepartment(payload: { id?: number; name: string; description: string; isActive: boolean }) {
    try {
      if (payload.id) await adminApi.updateDepartment(payload.id, payload); else await adminApi.createDepartment(payload);
      setSelectedDepartment(null); setNotice({ kind: 'success', text: payload.id ? 'Отдел обновлён.' : 'Отдел создан.' }); await load();
    } catch (error) { setNotice({ kind: 'error', text: error instanceof Error ? error.message : 'Не удалось сохранить отдел.' }); }
  }

  async function savePosition(payload: { id?: number; name: string; isManagerPosition: boolean; isActive: boolean }) {
    try {
      if (payload.id) await adminApi.updatePosition(payload.id, payload); else await adminApi.createPosition(payload);
      setSelectedPosition(null); setNotice({ kind: 'success', text: payload.id ? 'Должность обновлена.' : 'Должность создана.' }); await load();
    } catch (error) { setNotice({ kind: 'error', text: error instanceof Error ? error.message : 'Не удалось сохранить должность.' }); }
  }

  async function resetPassword(target: UserListItem, newPassword: string, convertToLocal: boolean) {
    try {
      setBusy(true);
      await adminApi.resetUserPassword(target.id, newPassword, convertToLocal);
      setPasswordTarget(null);
      setNotice({ kind: 'success', text: convertToLocal ? 'Пароль установлен, учётная запись переведена в Local.' : 'Пароль изменён.' });
      await load();
    } catch (error) {
      setNotice({ kind: 'error', text: error instanceof Error ? error.message : 'Не удалось изменить пароль.' });
    } finally {
      setBusy(false);
    }
  }

  const stats = useMemo(() => ({
    users: users.length,
    activeUsers: users.filter(x => x.isActive).length,
    departments: departments.length,
    positions: positions.length,
  }), [users, departments, positions]);

  if (!allowedUsers && !allowedDepartments && !allowedPositions) {
    return <section className="access-denied"><div><span className="page-kicker">ACCESS</span><h1>Недостаточно прав</h1><p>У вашей роли нет разрешений на административный раздел.</p></div></section>;
  }

  return (
    <div className="admin-page">
      <header className="page-header page-header-with-back">
        <div className="admin-heading-left"><button className="page-back-button" type="button" onClick={() => window.history.length > 1 ? window.history.back() : (window.location.hash = '#dashboard')}>← Назад</button><div><span className="page-kicker">ADMINISTRATION</span><h1>Структура компании</h1><p>Пользователи, подразделения и должности — в одном рабочем пространстве.</p></div></div>
        <button className="ghost-button" type="button" onClick={() => void load()} disabled={busy}>↻ Обновить</button>
      </header>

      {notice && <div className={`notice ${notice.kind}`}>{notice.text}</div>}

      <section className="stats-row">
        <div className="stat-card"><span>Сотрудники</span><strong>{stats.users}</strong><small>{stats.activeUsers} активных</small></div>
        <div className="stat-card"><span>Подразделения</span><strong>{stats.departments}</strong><small>Организационная структура</small></div>
        <div className="stat-card"><span>Должности</span><strong>{stats.positions}</strong><small>Профили должностей</small></div>
      </section>

      <div className="admin-tabs">
        {allowedUsers && <button className={tab === 'users' ? 'active' : ''} onClick={() => setTab('users')}>Пользователи</button>}
        {allowedDepartments && <button className={tab === 'departments' ? 'active' : ''} onClick={() => setTab('departments')}>Отделы</button>}
        {allowedPositions && <button className={tab === 'positions' ? 'active' : ''} onClick={() => setTab('positions')}>Должности</button>}
      </div>

      {tab === 'users' && allowedUsers && <UsersTab users={users} departments={departments} positions={positions} roles={roles} canCreate={hasPermission(permissions.usersCreate)} canEdit={hasPermission(permissions.usersEdit)} canDelete={hasPermission(permissions.usersDelete)} onCreate={() => setUserEditor(emptyUser)} onOpen={openUser} onEdit={(u) => setUserEditor({ ...emptyUser, ...u, id: u.id, login: u.login, displayName: u.displayName, email: u.email ?? '', phone: '', departmentId: u.departmentId ?? 0, positionId: u.positionId ?? 0, managerId: u.managerId ?? 0, isActive: u.isActive })} onDelete={removeUser} onPassword={setPasswordTarget} />}
      {tab === 'departments' && allowedDepartments && <SimpleDepartmentTab departments={departments} canManage={canManageDepartments} onCreate={() => setSelectedDepartment({ id: 0, name: '', description: '', isActive: true, userCount: 0 })} onEdit={setSelectedDepartment} onDelete={async d => { if (!window.confirm(`Деактивировать отдел «${d.name}»?`)) return; await adminApi.deleteDepartment(d.id); setNotice({ kind: 'success', text: 'Отдел деактивирован.' }); await load(); }} />}
      {tab === 'positions' && allowedPositions && <SimplePositionTab positions={positions} canManage={canManagePositions} onCreate={() => setSelectedPosition({ id: 0, name: '', isManagerPosition: false, isActive: true, userCount: 0 })} onEdit={setSelectedPosition} onDelete={async p => { if (!window.confirm(`Деактивировать должность «${p.name}»?`)) return; await adminApi.deletePosition(p.id); setNotice({ kind: 'success', text: 'Должность деактивирована.' }); await load(); }} />}

      {userEditor && <UserEditor state={userEditor} users={users} departments={departments} positions={positions} onCancel={() => setUserEditor(null)} onSave={saveUser} />}
      {selectedUser && <UserDetailsPanel user={selectedUser} roles={roles} canManageRoles={hasPermission(permissions.rolesManage)} onClose={() => setSelectedUser(null)} onRolesSaved={async () => { setSelectedUser(await adminApi.getUser(selectedUser.id)); }} />}
      {passwordTarget && <PasswordResetModal user={passwordTarget} onCancel={() => setPasswordTarget(null)} onSave={(password, convertToLocal) => resetPassword(passwordTarget, password, convertToLocal)} />}
      {selectedDepartment && <DepartmentEditor department={selectedDepartment.id ? selectedDepartment : null} onCancel={() => setSelectedDepartment(null)} onSave={saveDepartment} />}
      {selectedPosition && <PositionEditor position={selectedPosition.id ? selectedPosition : null} onCancel={() => setSelectedPosition(null)} onSave={savePosition} />}
    </div>
  );
}

interface UserEditorState { id?: number; login: string; displayName: string; email: string; phone: string; departmentId: number; positionId: number; managerId: number; isActive: boolean; }
const emptyUser: UserEditorState = { login: '', displayName: '', email: '', phone: '', departmentId: 0, positionId: 0, managerId: 0, isActive: true };

function UsersTab({ users, departments, positions, canCreate, canEdit, canDelete, onCreate, onOpen, onEdit, onDelete, onPassword }: { users: UserListItem[]; departments: Department[]; positions: Position[]; roles: Role[]; canCreate: boolean; canEdit: boolean; canDelete: boolean; onCreate: () => void; onOpen: (id: number) => void; onEdit: (u: UserListItem) => void; onDelete: (u: UserListItem) => void; onPassword: (u: UserListItem) => void; }) {
  const [query, setQuery] = useState(''); const [departmentId, setDepartmentId] = useState(0); const [activeOnly, setActiveOnly] = useState(true);
  const filtered = users.filter(u => (!activeOnly || u.isActive) && (!departmentId || u.departmentId === departmentId) && `${u.login} ${u.displayName} ${u.email ?? ''}`.toLowerCase().includes(query.toLowerCase()));
  return <section className="data-section"><div className="section-toolbar"><div className="filters"><input value={query} onChange={e => setQuery(e.target.value)} placeholder="Поиск сотрудника…" /><select value={departmentId} onChange={e => setDepartmentId(Number(e.target.value))}><option value={0}>Все отделы</option>{departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}</select><label className="check"><input type="checkbox" checked={activeOnly} onChange={e => setActiveOnly(e.target.checked)} /> Только активные</label></div>{canCreate && <button className="primary-button" onClick={onCreate}>＋ Добавить сотрудника</button>}</div><div className="table-card"><table><thead><tr><th>Сотрудник</th><th>Отдел</th><th>Должность</th><th>Руководитель</th><th>Статус</th><th /></tr></thead><tbody>{filtered.map(u => <tr key={u.id} onDoubleClick={() => onOpen(u.id)}><td><button className="row-link" onClick={() => onOpen(u.id)}><span className="avatar">{u.displayName.slice(0,1).toUpperCase()}</span><span><strong>{u.displayName}</strong><small>{u.login}</small></span></button></td><td>{u.departmentName ?? '—'}</td><td>{u.positionName ?? '—'}</td><td>{u.managerName ?? '—'}</td><td><span className={u.isActive ? 'badge success' : 'badge muted'}>{u.isActive ? 'Активен' : 'Неактивен'}</span></td><td className="actions">{canEdit && <button onClick={() => onEdit(u)}>Изменить</button>}{canEdit && u.isActive && <button onClick={() => onPassword(u)}>Пароль</button>}{canDelete && u.isActive && <button className="danger-text" onClick={() => onDelete(u)}>Отключить</button>}</td></tr>)}</tbody></table>{filtered.length === 0 && <div className="empty-state">Сотрудники не найдены.</div>}</div></section>;
}

function SimpleDepartmentTab({ departments, canManage, onCreate, onEdit, onDelete }: { departments: Department[]; canManage: boolean; onCreate: () => void; onEdit: (d: Department) => void; onDelete: (d: Department) => void }) { return <section className="data-section"><div className="section-toolbar"><div><h2>Подразделения</h2><p>Организационные единицы компании.</p></div>{canManage && <button className="primary-button" onClick={onCreate}>＋ Добавить отдел</button>}</div><div className="card-grid">{departments.map(d => <article className="entity-card" key={d.id}><div className="entity-icon">⌁</div><div className="entity-copy"><span className="badge muted">{d.isActive ? 'Активен' : 'Неактивен'}</span><h3>{d.name}</h3><p>{d.description || 'Без описания'}</p><strong>{d.userCount} сотрудников</strong></div>{canManage && <div className="card-actions"><button onClick={() => onEdit(d)}>Изменить</button>{d.isActive && <button className="danger-text" onClick={() => onDelete(d)}>Отключить</button>}</div>}</article>)}</div>{departments.length === 0 && <div className="empty-state">Отделов пока нет.</div>}</section>; }

function SimplePositionTab({ positions, canManage, onCreate, onEdit, onDelete }: { positions: Position[]; canManage: boolean; onCreate: () => void; onEdit: (p: Position) => void; onDelete: (p: Position) => void }) { return <section className="data-section"><div className="section-toolbar"><div><h2>Должности</h2><p>Роли в организационной структуре.</p></div>{canManage && <button className="primary-button" onClick={onCreate}>＋ Добавить должность</button>}</div><div className="card-grid">{positions.map(p => <article className="entity-card" key={p.id}><div className="entity-icon">⌘</div><div className="entity-copy"><div className="inline-badges"><span className="badge muted">{p.isActive ? 'Активна' : 'Неактивна'}</span>{p.isManagerPosition && <span className="badge manager">Руководящая</span>}</div><h3>{p.name}</h3><p>Пользователей: {p.userCount}</p></div>{canManage && <div className="card-actions"><button onClick={() => onEdit(p)}>Изменить</button>{p.isActive && <button className="danger-text" onClick={() => onDelete(p)}>Отключить</button>}</div>}</article>)}</div>{positions.length === 0 && <div className="empty-state">Должностей пока нет.</div>}</section>; }

function UserEditor({ state, users, departments, positions, onCancel, onSave }: { state: UserEditorState; users: UserListItem[]; departments: Department[]; positions: Position[]; onCancel: () => void; onSave: (state: UserEditorState) => Promise<void> }) { const [form,setForm]=useState(state); const set=(k: keyof UserEditorState,v: string|number|boolean)=>setForm(x=>({...x,[k]:v})); return <Modal title={form.id ? 'Редактирование сотрудника' : 'Новый сотрудник'} onClose={onCancel}><div className="form-grid"><label>Логин<input disabled={Boolean(form.id)} value={form.login} onChange={e=>set('login',e.target.value)} /></label><label>Отображаемое имя<input value={form.displayName} onChange={e=>set('displayName',e.target.value)} /></label><label>Email<input type="email" value={form.email} onChange={e=>set('email',e.target.value)} /></label><label>Телефон<input value={form.phone} onChange={e=>set('phone',e.target.value)} /></label><label>Отдел<select value={form.departmentId} onChange={e=>set('departmentId',Number(e.target.value))}><option value={0}>Не назначен</option>{departments.filter(x=>x.isActive).map(d=><option key={d.id} value={d.id}>{d.name}</option>)}</select></label><label>Должность<select value={form.positionId} onChange={e=>set('positionId',Number(e.target.value))}><option value={0}>Не назначена</option>{positions.filter(x=>x.isActive).map(p=><option key={p.id} value={p.id}>{p.name}</option>)}</select></label><label className="full">Руководитель<select value={form.managerId} onChange={e=>set('managerId',Number(e.target.value))}><option value={0}>Не назначен</option>{users.filter(x=>x.isActive && x.id!==form.id).map(u=><option key={u.id} value={u.id}>{u.displayName}</option>)}</select></label><label className="check full"><input type="checkbox" checked={form.isActive} onChange={e=>set('isActive',e.target.checked)} /> Пользователь активен</label></div><div className="modal-actions"><button className="ghost-button" onClick={onCancel}>Отмена</button><button className="primary-button" disabled={!form.displayName.trim() || (!form.id && !form.login.trim())} onClick={()=>void onSave(form)}>Сохранить</button></div></Modal>; }

function DepartmentEditor({ department, onCancel, onSave }: { department: Department | null; onCancel: () => void; onSave: (p:{id?:number;name:string;description:string;isActive:boolean})=>Promise<void> }) { const [name,setName]=useState(department?.name??'');const[description,setDescription]=useState(department?.description??'');const[isActive,setActive]=useState(department?.isActive??true);return <Modal title={department?'Редактирование отдела':'Новый отдел'} onClose={onCancel}><div className="form-grid"><label className="full">Название<input value={name} onChange={e=>setName(e.target.value)} /></label><label className="full">Описание<textarea value={description} onChange={e=>setDescription(e.target.value)} rows={4}/></label><label className="check full"><input type="checkbox" checked={isActive} onChange={e=>setActive(e.target.checked)} /> Отдел активен</label></div><div className="modal-actions"><button className="ghost-button" onClick={onCancel}>Отмена</button><button className="primary-button" disabled={!name.trim()} onClick={()=>void onSave({id:department?.id,name:name.trim(),description,isActive})}>Сохранить</button></div></Modal>; }

function PositionEditor({ position, onCancel, onSave }: { position: Position | null; onCancel: () => void; onSave: (p:{id?:number;name:string;isManagerPosition:boolean;isActive:boolean})=>Promise<void> }) { const[name,setName]=useState(position?.name??'');const[manager,setManager]=useState(position?.isManagerPosition??false);const[active,setActive]=useState(position?.isActive??true);return <Modal title={position?'Редактирование должности':'Новая должность'} onClose={onCancel}><div className="form-grid"><label className="full">Название<input value={name} onChange={e=>setName(e.target.value)} /></label><label className="check full"><input type="checkbox" checked={manager} onChange={e=>setManager(e.target.checked)} /> Руководящая должность</label><label className="check full"><input type="checkbox" checked={active} onChange={e=>setActive(e.target.checked)} /> Должность активна</label></div><div className="modal-actions"><button className="ghost-button" onClick={onCancel}>Отмена</button><button className="primary-button" disabled={!name.trim()} onClick={()=>void onSave({id:position?.id,name:name.trim(),isManagerPosition:manager,isActive:active})}>Сохранить</button></div></Modal>; }

function UserDetailsPanel({ user, roles, canManageRoles, onClose, onRolesSaved }: { user: UserDetails; roles: Role[]; canManageRoles: boolean; onClose: () => void; onRolesSaved: () => Promise<void> }) { const [selected,setSelected]=useState(user.roles.map(r=>r.id));const[busy,setBusy]=useState(false);return <div className="drawer-backdrop" onMouseDown={onClose}><aside className="details-drawer" onMouseDown={e=>e.stopPropagation()}><button className="drawer-close" onClick={onClose}>×</button><span className="page-kicker">ПРОФИЛЬ СОТРУДНИКА</span><div className="profile-hero"><div className="profile-avatar">{user.displayName.slice(0,1).toUpperCase()}</div><div><h2>{user.displayName}</h2><p>{user.login}</p></div></div><div className="detail-grid"><div><span>Отдел</span><strong>{user.departmentName??'Не назначен'}</strong></div><div><span>Должность</span><strong>{user.positionName??'Не назначена'}</strong></div><div><span>Руководитель</span><strong>{user.managerName??'Не назначен'}</strong></div><div><span>Email</span><strong>{user.email??'—'}</strong></div></div><section className="drawer-section"><div className="section-heading"><h3>Роли</h3>{canManageRoles&&<button className="primary-button small" disabled={busy} onClick={async()=>{setBusy(true);try{await adminApi.setUserRoles(user.id,selected);await onRolesSaved()}finally{setBusy(false)}}}>Сохранить</button>}</div>{roles.map(r=><label className="role-row" key={r.id}><input disabled={!canManageRoles} type="checkbox" checked={selected.includes(r.id)} onChange={e=>setSelected(s=>e.target.checked?[...s,r.id]:s.filter(x=>x!==r.id))}/><span><strong>{r.name}</strong><small>{r.description??'Без описания'}</small></span></label>)}</section><section className="drawer-section"><div className="section-heading"><h3>Подчинённые</h3><span className="badge muted">{user.subordinates.length}</span></div>{user.subordinates.length?user.subordinates.map(x=><div className="subordinate-row" key={x.id}><div className="avatar">{x.displayName.slice(0,1).toUpperCase()}</div><span><strong>{x.displayName}</strong><small>{x.positionName??'Должность не назначена'}</small></span></div>):<p className="muted-text">Подчинённых нет.</p>}</section></aside></div>; }

function PasswordResetModal({ user, onCancel, onSave }: { user: UserListItem; onCancel: () => void; onSave: (password: string, convertToLocal: boolean) => Promise<void> }) {
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [convertToLocal, setConvertToLocal] = useState(false);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);
  const submit = async () => {
    if (password.length < 8) { setError('Пароль должен содержать минимум 8 символов.'); return; }
    if (password !== confirm) { setError('Пароли не совпадают.'); return; }
    try { setSaving(true); setError(''); await onSave(password, convertToLocal); } catch (e) { setError(e instanceof Error ? e.message : 'Не удалось изменить пароль.'); } finally { setSaving(false); }
  };
  return <Modal title={`Пароль · ${user.displayName}`} onClose={onCancel}>
    <div className="password-help"><strong>{user.login}</strong><span>Windows-пользователь не использует пароль TaskManager, пока не переведён в Local.</span></div>
    {error && <div className="notice error">{error}</div>}
    <div className="form-grid">
      <label className="full">Новый пароль<input type="password" value={password} onChange={e=>setPassword(e.target.value)} autoFocus /></label>
      <label className="full">Повторите пароль<input type="password" value={confirm} onChange={e=>setConfirm(e.target.value)} /></label>
      <label className="check full"><input type="checkbox" checked={convertToLocal} onChange={e=>setConvertToLocal(e.target.checked)} /> Перевести Windows-учётную запись в Local</label>
    </div>
    <div className="modal-actions"><button className="ghost-button" onClick={onCancel}>Отмена</button><button className="primary-button" disabled={saving} onClick={()=>void submit()}>{saving?'Сохранение...':'Сохранить пароль'}</button></div>
  </Modal>;
}

function Modal({ title, children, onClose }: { title: string; children: ReactNode; onClose: () => void }) { return <div className="modal-backdrop" onMouseDown={onClose}><div className="modal" onMouseDown={e=>e.stopPropagation()}><div className="modal-header"><div><span className="page-kicker">TASKMANAGER</span><h2>{title}</h2></div><button className="drawer-close" onClick={onClose}>×</button></div>{children}</div></div>; }
