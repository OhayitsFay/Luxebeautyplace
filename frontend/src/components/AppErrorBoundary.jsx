import { Component } from 'react';

export default class AppErrorBoundary extends Component {
  state = { error: null };

  static getDerivedStateFromError(error) {
    return { error };
  }

  render() {
    if (this.state.error) {
      return (
        <main className="page section app-error">
          <span className="eyebrow">Application Error</span>
          <h1>The site could not finish loading.</h1>
          <p>{this.state.error.message || 'Refresh the page, then check the browser console if this continues.'}</p>
        </main>
      );
    }

    return this.props.children;
  }
}
