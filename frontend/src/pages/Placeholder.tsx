import { usePageTitle } from '../lib/usePageTitle';

/** Temporary page body until each route is implemented. */
export function Placeholder({ title }: { title: string }) {
  usePageTitle(title);
  return (
    <section>
      <h1>{title}</h1>
      <p className="muted">শীঘ্রই আসছে।</p>
    </section>
  );
}
