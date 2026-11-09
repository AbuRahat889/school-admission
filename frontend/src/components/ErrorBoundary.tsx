import { Component, type ErrorInfo, type ReactNode } from 'react';

/** Last line of defence against a blank page if a render throws. */
export class ErrorBoundary extends Component<{ children: ReactNode }, { failed: boolean }> {
  state = { failed: false };

  static getDerivedStateFromError() {
    return { failed: true };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Render error', error, info.componentStack);
  }

  render() {
    if (!this.state.failed) return this.props.children;
    return (
      <main className="container">
        <div className="banner banner-error" role="alert">
          কিছু একটা ভুল হয়েছে। পাতাটি আবার লোড করুন।
        </div>
        <button type="button" className="btn" onClick={() => window.location.reload()}>
          আবার লোড করুন
        </button>
      </main>
    );
  }
}
