import React from 'react';
import ReactDOM from 'react-dom/client';
import { Provider } from 'react-redux';
import App from './App.jsx';
import AppErrorBoundary from './components/AppErrorBoundary.jsx';
import { store } from './store/store.js';
import './styles.css';

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <AppErrorBoundary>
      <Provider store={store}>
        <App />
      </Provider>
    </AppErrorBoundary>
  </React.StrictMode>
);
