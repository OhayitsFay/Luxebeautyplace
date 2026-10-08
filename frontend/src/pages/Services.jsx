import { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { api } from '../api/client.js';
import { mapServiceFromApi } from '../api/mappers.js';
import ServiceCard from '../components/ServiceCard.jsx';

export default function Services() {
  const fallbackServices = useSelector((state) => state.salon.services);
  const [services, setServices] = useState(fallbackServices);
  const imageByName = new Map(fallbackServices.map((service) => [service.name, service.image]));

  useEffect(() => {
    let active = true;

    api.get('/services')
      .then((response) => {
        if (active) {
          setServices(response.data.map((service) => {
            const mapped = mapServiceFromApi(service);
            return { ...mapped, image: imageByName.get(mapped.name) ?? mapped.image };
          }));
        }
      })
      .catch(() => {
        if (active) setServices(fallbackServices);
      });

    return () => {
      active = false;
    };
  }, [fallbackServices]);

  const groupedServices = services.reduce((groups, service) => {
    const category = service.category || 'Services';
    groups[category] = groups[category] ? [...groups[category], service] : [service];
    return groups;
  }, {});

  return (
    <section className="page section">
      <div className="page-heading">
        <span className="eyebrow">Services</span>
        <h1>Professional salon services with clear pricing.</h1>
        <p>Choose a service to start a booking. Administrators can manage services, descriptions, images, pricing, and duration from the dashboard.</p>
      </div>
      {Object.entries(groupedServices).map(([category, categoryServices]) => (
        <section className="service-category" key={category}>
          <h2>{category}</h2>
          <div className="cards service-grid">
            {categoryServices.map((service) => <ServiceCard key={service.id} service={service} />)}
          </div>
        </section>
      ))}
    </section>
  );
}
