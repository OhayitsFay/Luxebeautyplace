import { Instagram, Mail, MapPin, MessageCircle, Phone } from 'lucide-react';

export default function Contact() {
  return (
    <section className="page section contact-layout">
      <div>
        <span className="eyebrow">Contact</span>
        <h1>Visit, call, message, or book your beauty day online.</h1>
        <p>Business details are placeholder-ready so the salon can update them before launch.</p>
        <div className="contact-list">
          <span><MapPin size={18} /> [BUSINESS ADDRESS]</span>
          <span><Phone size={18} /> [PHONE NUMBER]</span>
          <span><MessageCircle size={18} /> [WHATSAPP NUMBER]</span>
          <span><Mail size={18} /> [EMAIL ADDRESS]</span>
          <span>[OPENING HOURS]</span>
        </div>
      </div>
      <div className="form-panel">
        <h2>Social links</h2>
        <a className="social-link" href="https://instagram.com/luxebeautyplace"><Instagram size={18} /> luxebeautyplace</a>
        <a className="social-link" href="https://instagram.com/luxebeautyplace_academy"><Instagram size={18} /> luxebeautyplace_academy</a>
        <a className="social-link" href="https://tiktok.com/@luxebeautyplace">TikTok luxebeautyplace</a>
        <a className="social-link" href="https://tiktok.com/@luxebeautyplace_academy">TikTok luxebeautyplace_academy</a>
        <a className="social-link" href="#">[GOOGLE BUSINESS PROFILE LINK]</a>
      </div>
    </section>
  );
}
