import { Link } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { ArrowRight, BadgeCheck, CalendarDays, Star } from 'lucide-react';
import { useSelector } from 'react-redux';
import { api } from '../api/client.js';
import { mapServiceFromApi, mapTestimonialFromApi } from '../api/mappers.js';
import ServiceCard from '../components/ServiceCard.jsx';

export default function Home() {
  const { services: fallbackServices, testimonials: fallbackTestimonials } = useSelector((state) => state.salon);
  const [services, setServices] = useState(fallbackServices);
  const [error, setError] = useState('');
  const [testimonials, setTestimonials] = useState(fallbackTestimonials);
  const [reviewForm, setReviewForm] = useState({ name: '', rating: 5, review: '' });
  const [sent, setSent] = useState(false);
  const featured = services.slice(0, 3);

  useEffect(() => {
    let active = true;
    api.get('/services').then(({ data }) => {
      if (active) setServices(data.map(mapServiceFromApi));
    }).catch(() => {});
    return () => { active = false; };
  }, []);

  useEffect(() => {
    let active = true;

    api.get('/testimonials')
      .then((response) => {
        if (active) setTestimonials(response.data.map(mapTestimonialFromApi));
      })
      .catch(() => {
        if (active) setTestimonials(fallbackTestimonials);
      });

    return () => {
      active = false;
    };
  }, [fallbackTestimonials]);

  const updateReview = (event) => {
    setReviewForm((current) => ({ ...current, [event.target.name]: event.target.value }));
  };

  const submitReview = async (event) => {
    event.preventDefault();
    setError('');
    setSent(false);
    const review = { ...reviewForm, rating: Number(reviewForm.rating) };

    try {
      await api.post('/testimonials', review);
    } catch {
      setError('Your review was not sent. Please try again when the server is available.');
      return;
    }

    setReviewForm({ name: '', rating: 5, review: '' });
    setSent(true);
  };

  return (
    <>
      <section className="hero">
        <div className="hero-content">
          <span className="eyebrow">Black. Gold. Beautiful.</span>
          <h1>Luxe Beauty Place</h1>
          <p>Premium salon services and seamless appointment booking for hair, nails, makeup, lashes, brows, and piercings.</p>
          <div className="hero-actions">
            <Link className="button" to="/services"><CalendarDays size={18} /> Book Appointment</Link>
            <Link className="button secondary" to="/contact">Contact Us</Link>
          </div>
        </div>
        <div className="hero-panel">
          <img src="/photo_2026-08-20_00-29-03.jpg" alt="Modern luxury salon interior" />
          <div className="offer-strip">
            <BadgeCheck size={19} />
            <span>Special offer: full glam package consultations now available.</span>
          </div>
        </div>
      </section>

      <section className="section intro-grid">
        <div>
          <span className="eyebrow">Salon Experience</span>
          <h2>Designed for polished beauty days and effortless rebooking.</h2>
        </div>
        <p>Luxe Beauty Place brings service browsing, pricing, appointments, reviews, and client communication into one modern experience.</p>
      </section>

      <section className="section">
        <div className="section-heading">
          <div>
            <span className="eyebrow">Featured Services</span>
            <h2>Signature treatments</h2>
          </div>
          <Link className="text-link" to="/services">View all <ArrowRight size={17} /></Link>
        </div>
        <div className="cards three">
          {featured.map((service) => <ServiceCard key={service.id} service={service} />)}
        </div>
      </section>

      <section className="band testimonials-layout">
        <div>
          <span className="eyebrow">Client Love</span>
          <h2>Reviews from clients who trust the finish.</h2>
          <form className="form-panel review-form" onSubmit={submitReview}>
            <h2>Submit a review</h2>
            <label>
              Name
              <input required name="name" value={reviewForm.name} onChange={updateReview} />
            </label>
            <label>
              Rating
              <select name="rating" value={reviewForm.rating} onChange={updateReview}>
                {[5, 4, 3, 2, 1].map((rating) => <option key={rating} value={rating}>{rating}</option>)}
              </select>
            </label>
            <label>
              Review
              <textarea required name="review" value={reviewForm.review} onChange={updateReview} rows="5" />
            </label>
            <button className="button full" type="submit">Send Review</button>
            {sent && <p className="success">Review sent for admin approval.</p>}
            {error && <p className="form-error" role="alert">{error}</p>}
          </form>
        </div>
        <div className="review-list">
          {testimonials.map((item) => (
            <blockquote key={item.id}>
              <div className="stars">{Array.from({ length: item.rating }).map((_, index) => <Star key={index} size={15} fill="currentColor" />)}</div>
              <p>{item.review}</p>
              <cite>{item.name} - {item.status}</cite>
            </blockquote>
          ))}
        </div>
      </section>
    </>
  );
}
