import { configureStore } from '@reduxjs/toolkit';
import salonReducer from './salonSlice.js';

export const store = configureStore({
  reducer: {
    salon: salonReducer
  }
});
