import { Link } from 'react-router-dom';
import { CalendarDays } from 'lucide-react';
import { formatCurrency } from '../data/salonData.js';

const fallbackImage = '/service-images/photo_2026-08-20_18-30-46.jpg';

export default function ServiceCard({ service }) {
  return (
    <article className="service-card">
      <img
        src={service.image || fallbackImage}
        alt={service.name}
        onError={(event) => {
          event.currentTarget.src = fallbackImage;
        }}
      />
      <div className="service-card-body">
        <span className="eyebrow">{service.category}</span>
        <h3>{service.name}</h3>
        <p>{service.description}</p>
        <div className="card-meta">
          <strong>{service.priceLabel || formatCurrency(service.price)}</strong>
          <span>{service.duration} min</span>
        </div>
        <Link className="button full" to={`/booking/${service.id}`}>
          <CalendarDays size={18} />
          Book Service
        </Link>
      </div>
    </article>
  );
}
