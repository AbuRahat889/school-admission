import { NavLink, Outlet, Route, Routes } from 'react-router-dom';
import { NotFoundPage } from './pages/NotFoundPage';
import { Placeholder } from './pages/Placeholder';

function Layout() {
  return (
    <>
      <header className="topbar">
        <div className="container topbar-inner">
          <span className="brand">স্কুল ভর্তি</span>
          <nav className="nav">
            <NavLink to="/" end>ক্লাস তালিকা</NavLink>
            <NavLink to="/classes/new">নতুন ক্লাস তৈরি</NavLink>
            <NavLink to="/me">আমার আবেদন</NavLink>
          </nav>
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
        <Route path="/" element={<Placeholder title="ক্লাস তালিকা" />} />
        <Route path="/classes/new" element={<Placeholder title="নতুন ক্লাস তৈরি" />} />
        <Route path="/holds/:holdId" element={<Placeholder title="আবেদনের অবস্থা" />} />
        <Route path="/me" element={<Placeholder title="আমার আবেদন" />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
