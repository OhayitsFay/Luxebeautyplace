import { NavLink, Outlet, Link } from 'react-router-dom';
import { Instagram, KeyRound, Menu, MessageCircle } from 'lucide-react';
import { useState } from 'react';

const links = [
  ['/', 'Home'],
  ['/about', 'About'],
  ['/services', 'Services'],
  ['/contact', 'Contact']
];

export default function Layout() {
  const [open, setOpen] = useState(false);

  return (
    <div className="site-shell">
      <header className="topbar">
        <Link className="brand" to="/" aria-label="Luxe Beauty Place home">
          <span className="brand-mark"><img src="/logo.jpg" alt="Luxe Beauty Place" /></span>
          <span>
            <strong>Luxe Beauty Place</strong>
            <small>BEAUTY STYLE CONFIDENCE</small>
          </span>
        </Link>
        <button className="icon-button menu-button" onClick={() => setOpen((value) => !value)} aria-label="Toggle navigation">
          <Menu size={22} />
        </button>
        <nav className={open ? 'nav open' : 'nav'}>
          {links.map(([to, label]) => (
            <NavLink key={to} to={to} onClick={() => setOpen(false)}>
              {label}
            </NavLink>
          ))}
        </nav>
      </header>
      <main>
        <Outlet />
      </main>
      <footer className="footer">
        <div>
          <h2>Luxe Beauty Place</h2>
          <img src="/logo.jpg" alt="Luxe Beauty Place" width={200} height={170} />
          <p>BEAUTY STYLE CONFIDENCE</p>
        </div>
        <div className="footer-grid">
          <span>319B, New Oko-Oba Road, 
            Opposite Maple Wood Estate 
            Beside Mobile Station 
            Lagos, Nigeria</span>
          <span>+2348153144870</span>
          <span>[EMAIL ADDRESS]</span>
          <span>[OPENING HOURS]</span>
        </div>
        <div className="social-row">
          <a href="https://instagram.com/luxebeautyplace" aria-label="Instagram luxebeautyplace"><Instagram size={18} /> luxebeautyplace</a>
          <a href="https://tiktok.com/@luxebeautyplace" aria-label="TikTok luxebeautyplace">TikTok</a>
          <a href="https://wa.me/" aria-label="WhatsApp"><MessageCircle size={18} /> WhatsApp</a>
          <Link className="staff-entry" to="/admin" aria-label="Staff sign in">
            <KeyRound size={16} />
          </Link>
        </div>
      </footer>
    </div>
  );
}
