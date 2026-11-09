import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App';
import { ErrorBoundary } from './components/ErrorBoundary';
import { StudentIdProvider } from './lib/studentIdContext';
import './styles.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ErrorBoundary>
      <StudentIdProvider>
        <BrowserRouter>
          <App />
        </BrowserRouter>
      </StudentIdProvider>
    </ErrorBoundary>
  </StrictMode>,
);
