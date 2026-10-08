const fallbackServiceImage = '/service-images/photo_2026-08-20_18-30-46.jpg';

export const normalizeImageUrl = (imageUrl) => {
  if (!imageUrl) return fallbackServiceImage;
  if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://') || imageUrl.startsWith('/')) return imageUrl;

  const publicIndex = imageUrl.lastIndexOf('public/');
  if (publicIndex >= 0) return `/${imageUrl.slice(publicIndex + 'public/'.length)}`;

  return `/${imageUrl.replace(/^src\/public\//, '').replace(/^public\//, '')}`;
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
