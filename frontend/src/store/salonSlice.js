import { createSlice } from '@reduxjs/toolkit';
import { galleryItems, services, testimonials } from '../data/salonData.js';

const initialState = {
  services,
  testimonials,
  galleryItems,
  bookings: []
};

const salonSlice = createSlice({
  name: 'salon',
  initialState,
  reducers: {
    addBooking: (state, action) => {
      state.bookings.unshift({
        id: crypto.randomUUID(),
        status: 'Pending',
        createdAt: new Date().toISOString(),
        ...action.payload
      });
    },
    addTestimonial: (state, action) => {
      state.testimonials.unshift({
        id: crypto.randomUUID(),
        status: 'Pending',
        ...action.payload
      });
    }
  }
});

export const { addBooking, addTestimonial } = salonSlice.actions;
export default salonSlice.reducer;
