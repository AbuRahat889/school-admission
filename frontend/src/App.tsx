import { NavLink, Outlet, Route, Routes } from 'react-router-dom';
import { StudentIdInput } from './components/StudentIdInput';
import { ClassListPage } from './pages/ClassListPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { CreateClassPage } from './pages/CreateClassPage';
import { Placeholder } from './pages/Placeholder';

function Layout() {
  return (
    <>
      <header className="topbar">
        <div className="container topbar-inner">
          <div className="topbar-left">
            <span className="brand">স্কুল ভর্তি</span>
            <nav className="nav">
              <NavLink to="/" end>ক্লাস তালিকা</NavLink>
              <NavLink to="/classes/new">নতুন ক্লাস তৈরি</NavLink>
              <NavLink to="/me">আমার আবেদন</NavLink>
            </nav>
          </div>
          <StudentIdInput />
        </div>
      </header>
      <main className="container">
        <Outlet />
      </main>
    </>
  );
}

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<ClassListPage />} />
        <Route path="/classes/new" element={<CreateClassPage />} />
        <Route path="/holds/:holdId" element={<Placeholder title="আবেদনের অবস্থা" />} />
        <Route path="/me" element={<Placeholder title="আমার আবেদন" />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
