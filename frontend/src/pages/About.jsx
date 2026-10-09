import { useSelector } from 'react-redux';
import { useEffect, useState } from 'react';
import { api } from '../api/client.js';
import { normalizeImageUrl } from '../api/mappers.js';

export default function About() {
  const fallbackGallery = useSelector((state) => state.salon.galleryItems);
  const [galleryItems, setGalleryItems] = useState(fallbackGallery);
  useEffect(() => {
    let active = true;
    api.get('/gallery').then(({ data }) => {
      if (active) setGalleryItems(data.map((item) => ({ id: item.galleryPhotoId, title: item.title, type: item.category, image: normalizeImageUrl(item.imageUrl) })));
    }).catch(() => {});
    return () => { active = false; };
  }, []);

  return (
    <>
      <section className="page section split">
        <div>
          <span className="eyebrow">About Us</span>
          <h1>Beauty services with a luxury standard and a warm client rhythm.</h1>
          <p>Luxe Beauty Place is built around refined styling, careful consultations, high hygiene standards, and a smooth appointment experience from first visit to loyal rebooking.</p>
        </div>
        <div className="story-stack">
          <article>
            <h2>Mission</h2>
            <p>To help every client leave polished, confident, and cared for through excellent salon artistry and reliable service.</p>
          </article>
          <article>
            <h2>Vision</h2>
            <p>To become a leading beauty destination for salon services, beauty education, and premium retail.</p>
          </article>
          <article>
            <h2>Team</h2>
            <p>Stylists, lash artists, makeup artists, nail technicians, and beauty specialists can be featured here with photos and bios.</p>
          </article>
        </div>
      </section>

      <section className="section">
        <div className="page-heading">
          <span className="eyebrow">Gallery</span>
          <h1>Interior, hair, lashes, nails, makeup, and beauty finishes.</h1>
        </div>
        <div className="gallery-grid">
          {galleryItems.map((item) => (
            <figure key={item.id}>
              <img src={item.image} alt={item.title} />
              <figcaption>
                <span>{item.type}</span>
                <strong>{item.title}</strong>
              </figcaption>
            </figure>
          ))}
        </div>
        {!galleryItems.length && <p>Salon gallery photos will appear here when published.</p>}
      </section>
    </>
  );
}
