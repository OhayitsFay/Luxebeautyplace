const fallbackServiceImage = '/service-images/photo_2026-08-20_18-30-46.jpg';

export const normalizeImageUrl = (imageUrl) => {
  if (typeof imageUrl !== 'string' || !imageUrl.trim()) return fallbackServiceImage;
  const path = imageUrl.trim().replaceAll('\\', '/');
  if (/^https?:\/\//i.test(path)) return path;
  // Older records reference source folders instead of Vite's public URLs.
  const serviceIndex = path.indexOf('service-images/');
  if (serviceIndex >= 0) return `/${path.slice(serviceIndex).replace('service-images/public/', 'service-images/')}`;
  const publicIndex = path.lastIndexOf('public/');
  if (publicIndex >= 0) return `/${path.slice(publicIndex + 'public/'.length)}`;
  if (/^[a-z]:/i.test(path) || /^[a-z][a-z0-9+.-]*:/i.test(path) || path.startsWith('//')) return fallbackServiceImage;
  return path.startsWith('/') ? path : `/${path}`;
};

export const mapServiceFromApi = (service) => ({
  id: String(service.serviceId),
  serviceId: service.serviceId,
  name: service.serviceName,
  category: service.category || 'Service',
  description: service.description,
  price: service.price,
  priceLabel: service.priceLabel,
  duration: service.duration,
  image: normalizeImageUrl(service.imageUrl)
});

export const mapTestimonialFromApi = (testimonial) => ({
  id: String(testimonial.testimonialId),
  testimonialId: testimonial.testimonialId,
  name: testimonial.name,
  review: testimonial.review,
  rating: testimonial.rating,
  status: testimonial.status
});
