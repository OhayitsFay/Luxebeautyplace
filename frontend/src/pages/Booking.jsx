import { useEffect, useMemo, useState } from 'react';
import { useSelector } from 'react-redux';
import { useParams } from 'react-router-dom';
import { api } from '../api/client.js';
import { mapServiceFromApi, normalizeImageUrl } from '../api/mappers.js';

export default function Booking() {
  const { serviceId } = useParams();
  const fallbackServices = useSelector((state) => state.salon.services);
  const [services, setServices] = useState(fallbackServices);
  const routeService = serviceId
    ? services.find((service) => service.id === serviceId || service.name === fallbackServices.find((item) => item.id === serviceId)?.name)
    : services[0];
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState('');
  const [form, setForm] = useState({
    serviceId: routeService?.id ?? '',
    date: '',
    time: '',
    stylist: '',
    fullName: '',
    email: '',
    phone: '',
    notes: ''
  });
  const selectedService = services.find((service) => service.id === form.serviceId) ?? routeService;

  const timeSlots = useMemo(() => ['09:00', '10:30', '12:00', '13:30', '15:00', '16:30'], []);

  useEffect(() => {
    let active = true;

    api.get('/services')
      .then((response) => {
        if (active) {
          setServices(response.data.map((service) => {
            const mapped = mapServiceFromApi(service);
            return mapped;
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

  useEffect(() => {
    if (routeService?.id) {
      setForm((current) => ({ ...current, serviceId: routeService.id }));
    }
  }, [routeService?.id]);

  const update = (event) => {
    setForm((current) => ({ ...current, [event.target.name]: event.target.value }));
  };

  const submit = async (event) => {
    event.preventDefault();
    setError('');
    setSubmitted(false);
    if (!Number.isInteger(Number(form.serviceId)) || Number(form.serviceId) <= 0) {
      setError('Booking is unavailable until services load from the server. Please try again.');
      return;
    }

    try {
      await api.post('/appointments', {
        serviceId: Number(form.serviceId),
        appointmentDate: form.date,
        appointmentTime: `${form.time}:00`,
        fullName: form.fullName,
        email: form.email,
        phone: form.phone,
        stylist: form.stylist || null,
        notes: form.notes || null
      });
      setSubmitted(true);
    } catch (error) {
      setError(error.response?.data?.message || 'Booking was not saved. Please try again when the server is available.');
    }
  };

  if (!selectedService) return <p className="page section">This service is unavailable. Please choose a service from the Services page.</p>;

  return (
    <section className="page section booking-layout">
      <div>
        <span className="eyebrow">Booking</span>
        <h1>Reserve your appointment.</h1>
        <p>Select a service, preferred date and time, then add your contact details. Confirmation can be wired to email and WhatsApp through the backend notification service.</p>
        <div className="booking-summary">
          <img
            src={normalizeImageUrl(selectedService.image)}
            alt={selectedService.name}
            onError={(event) => {
              const fallback = '/service-images/photo_2026-08-20_18-30-46.jpg';
              if (!event.currentTarget.src.endsWith(fallback)) event.currentTarget.src = fallback;
            }}
          />
          <div>
            <strong>{selectedService.name}</strong>
            <span>{selectedService.duration} minutes</span>
          </div>
        </div>
      </div>

      <form className="form-panel" onSubmit={submit}>
        <label>
          Service
          <select name="serviceId" value={form.serviceId} onChange={update}>
            {services.map((service) => <option key={service.id} value={service.id}>{service.name}</option>)}
          </select>
        </label>
        <div className="field-row">
          <label>
            Date
            <input required type="date" name="date" value={form.date} onChange={update} />
          </label>
          <label>
            Time
            <select required name="time" value={form.time} onChange={update}>
              <option value="">Select time</option>
              {timeSlots.map((slot) => <option key={slot} value={slot}>{slot}</option>)}
            </select>
          </label>
        </div>
        <label>
          Stylist
          <select name="stylist" value={form.stylist} onChange={update}>
            <option value="">Any available stylist</option>
            <option value="Lead Stylist">Lead Stylist</option>
            <option value="Nail Specialist">Nail Specialist</option>
            <option value="Lash Artist">Lash Artist</option>
          </select>
        </label>
        <div className="field-row">
          <label>
            Full name
            <input required name="fullName" value={form.fullName} onChange={update} />
          </label>
          <label>
            Phone
            <input required name="phone" value={form.phone} onChange={update} />
          </label>
        </div>
        <label>
          Email
          <input required type="email" name="email" value={form.email} onChange={update} />
        </label>
        <label>
          Notes
          <textarea name="notes" value={form.notes} onChange={update} rows="4" />
        </label>
        <button className="button full" type="submit">Confirm Booking</button>
        {submitted && <p className="success">Booking received and awaiting admin approval.</p>}
        {error && <p className="form-note">{error}</p>}
      </form>
    </section>
  );
}
