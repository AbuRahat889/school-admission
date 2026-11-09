import { Link } from 'react-router-dom';
import { usePageTitle } from '../lib/usePageTitle';

export function NotFoundPage() {
  usePageTitle('পাতাটি পাওয়া যায়নি');
  return (
    <section>
      <h1>পাতাটি পাওয়া যায়নি</h1>
      <p className="muted">আপনি যে পাতাটি খুঁজছেন সেটি নেই।</p>
      <Link to="/">ক্লাস তালিকায় ফিরে যান</Link>
    </section>
  );
}
