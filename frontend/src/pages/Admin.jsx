import { useEffect, useMemo, useState } from 'react';
import { CalendarCheck, Check, Edit3, LogOut, MessageSquareText, Plus, Save, Scissors, Trash2, UserPlus, X } from 'lucide-react';
import { api } from '../api/client.js';
import { formatCurrency } from '../data/salonData.js';

const emptyService = {
  serviceName: '',
  category: '',
  description: '',
  price: '',
  priceLabel: '',
  duration: '',
  imageUrl: ''
};

const emptyAdmin = {
  fullName: '',
  email: '',
  phone: '',
  password: ''
};

const statusOptions = ['Pending', 'Approved', 'Rescheduled', 'Cancelled', 'Completed'];

export default function Admin() {
  const [session, setSession] = useState(() => {
    const saved = localStorage.getItem('luxeAdminSession');
    return saved ? JSON.parse(saved) : null;
  });
  const [setupRequired, setSetupRequired] = useState(null);
  const [authMode, setAuthMode] = useState('login');
  const [login, setLogin] = useState({ email: '', password: '' });
  const [register, setRegister] = useState({ fullName: '', email: '', phone: '', password: '', setupCode: '', repairExisting: false });
  const [newAdmin, setNewAdmin] = useState(emptyAdmin);
  const [forgot, setForgot] = useState({ email: '', resetToken: '', newPassword: '' });
  const [changePassword, setChangePassword] = useState({ currentPassword: '', newPassword: '' });
  const [loginError, setLoginError] = useState('');
  const [services, setServices] = useState([]);
  const [testimonials, setTestimonials] = useState([]);
  const [appointments, setAppointments] = useState([]);
  const [adminAccounts, setAdminAccounts] = useState([]);
  const [serviceForm, setServiceForm] = useState(emptyService);
  const [editingServiceId, setEditingServiceId] = useState(null);
  const [notice, setNotice] = useState('');

  const authConfig = useMemo(() => ({
    headers: { Authorization: `Bearer ${session?.token}` }
  }), [session?.token]);

  useEffect(() => {
    api.get('/admin/setup-status')
      .then((response) => {
        setSetupRequired(response.data.setupRequired);
        if (response.data.setupRequired) setAuthMode('register');
      })
      .catch(() => {
        setSetupRequired(false);
      });
  }, []);

  const loadAdminData = async () => {
    if (!session?.token) return;

    const [serviceResponse, testimonialResponse, appointmentResponse, adminResponse] = await Promise.all([
      api.get('/services'),
      api.get('/admin/testimonials', authConfig),
      api.get('/appointments', authConfig),
      api.get('/admin/accounts', authConfig)
    ]);

    setServices(serviceResponse.data);
    setTestimonials(testimonialResponse.data);
    setAppointments(appointmentResponse.data);
    setAdminAccounts(adminResponse.data);
  };

  useEffect(() => {
    loadAdminData().catch(() => {
      setNotice('Could not load admin data. Check that the API is running and your session is valid.');
    });
  }, [session?.token]);

  const updateLogin = (event) => {
    setLogin((current) => ({ ...current, [event.target.name]: event.target.value }));
  };

  const updateRegister = (event) => {
    const value = event.target.type === 'checkbox' ? event.target.checked : event.target.value;
    setRegister((current) => ({ ...current, [event.target.name]: value }));
  };

  const updateNewAdmin = (event) => {
    setNewAdmin((current) => ({ ...current, [event.target.name]: event.target.value }));
  };

  const updateForgot = (event) => {
    setForgot((current) => ({ ...current, [event.target.name]: event.target.value }));
  };

  const updateChangePassword = (event) => {
    setChangePassword((current) => ({ ...current, [event.target.name]: event.target.value }));
  };

  const submitRegister = async (event) => {
    event.preventDefault();
    setLoginError('');

    try {
      const endpoint = register.repairExisting ? '/admin/setup-admin' : '/admin/register';
      await api.post(endpoint, register);
      setSetupRequired(false);
      setAuthMode('login');
      setLogin({ email: register.email, password: '' });
      setRegister({ fullName: '', email: '', phone: '', password: '', setupCode: '', repairExisting: false });
      setNotice('Admin account created. Sign in with the new password.');
    } catch {
      setLoginError('Admin setup failed. Check the setup code. If an old admin already exists, enable the repair option.');
    }
  };

  const submitLogin = async (event) => {
    event.preventDefault();
    setLoginError('');

    try {
      const response = await api.post('/admin/login', login);
      localStorage.setItem('luxeAdminSession', JSON.stringify(response.data));
      setSession(response.data);
      setLogin({ email: '', password: '' });
    } catch {
      api.get('/admin/setup-status')
        .then((response) => {
          if (response.data.setupRequired) {
            setAuthMode('register');
            setLoginError('No admin account exists yet. Create the first admin account.');
          } else {
            setLoginError('Admin sign in failed. Check the email and password.');
          }
        })
        .catch(() => {
          setLoginError('Admin sign in failed. Make sure the backend API is running.');
        });
    }
  };

  const logout = () => {
    localStorage.removeItem('luxeAdminSession');
    setSession(null);
  };

  const requestPasswordReset = async (event) => {
    event.preventDefault();
    setLoginError('');

    try {
      const response = await api.post('/admin/forgot-password', { email: forgot.email });
      setForgot((current) => ({ ...current, resetToken: response.data.resetToken ?? '' }));
      setNotice(response.data.resetToken ? 'Reset code created and filled in for local development.' : response.data.message);
    } catch {
      setLoginError('Could not create a reset code right now.');
    }
  };

  const submitPasswordReset = async (event) => {
    event.preventDefault();
    setLoginError('');

    try {
      await api.post('/admin/reset-password', forgot);
      setAuthMode('login');
      setLogin({ email: forgot.email, password: '' });
      setForgot({ email: '', resetToken: '', newPassword: '' });
      setNotice('Password reset. Sign in with the new password.');
    } catch {
      setLoginError('Password reset failed. Check the reset code.');
    }
  };

  const submitChangePassword = async (event) => {
    event.preventDefault();
    setNotice('');

    try {
      await api.post('/admin/change-password', changePassword, authConfig);
      setChangePassword({ currentPassword: '', newPassword: '' });
      setNotice('Password changed.');
    } catch {
      setNotice('Password change failed. Check the current password.');
    }
  };

  const submitNewAdmin = async (event) => {
    event.preventDefault();
    setNotice('');

    try {
      await api.post('/admin/accounts', newAdmin, authConfig);
      setNewAdmin(emptyAdmin);
      setNotice('Admin account created.');
      await loadAdminData();
    } catch {
      setNotice('Could not create admin account. Check that the email is not already used.');
    }
  };

  const updateServiceForm = (event) => {
    setServiceForm((current) => ({ ...current, [event.target.name]: event.target.value }));
  };

  const resetServiceForm = () => {
    setServiceForm(emptyService);
    setEditingServiceId(null);
  };

  const submitService = async (event) => {
    event.preventDefault();
    const payload = {
      ...serviceForm,
      price: Number(serviceForm.price),
      duration: Number(serviceForm.duration)
    };

    if (editingServiceId) {
      await api.put(`/services/${editingServiceId}`, { ...payload, serviceId: editingServiceId }, authConfig);
      setNotice('Service updated.');
    } else {
      await api.post('/services', payload, authConfig);
      setNotice('Service added.');
    }

    resetServiceForm();
    await loadAdminData();
  };

  const editService = (service) => {
    setEditingServiceId(service.serviceId);
    setServiceForm({
      serviceName: service.serviceName,
      category: service.category ?? '',
      description: service.description,
      price: service.price,
      priceLabel: service.priceLabel ?? '',
      duration: service.duration,
      imageUrl: service.imageUrl ?? ''
    });
  };

  const deleteService = async (serviceId) => {
    await api.delete(`/services/${serviceId}`, authConfig);
    setNotice('Service deleted.');
    await loadAdminData();
  };

  const setTestimonialStatus = async (testimonialId, status) => {
    await api.patch(`/testimonials/${testimonialId}/status/${status}`, null, authConfig);
    setNotice(`Review ${status.toLowerCase()}.`);
    await loadAdminData();
  };

  const deleteTestimonial = async (testimonialId) => {
    await api.delete(`/testimonials/${testimonialId}`, authConfig);
    setNotice('Review deleted.');
    await loadAdminData();
  };

  const setAppointmentStatus = async (appointmentId, status) => {
    await api.patch(`/appointments/${appointmentId}/status/${status}`, null, authConfig);
    setNotice(`Booking marked ${status.toLowerCase()}.`);
    await loadAdminData();
  };

  if (!session?.token) {
    return (
      <section className="page section admin-auth">
        <div className="form-panel admin-login">
          <span className="eyebrow">Staff Access</span>
          {authMode === 'register' ? (
            <form className="admin-form" onSubmit={submitRegister}>
              <h1>Create admin</h1>
              <label>
                Full name
                <input required name="fullName" value={register.fullName} onChange={updateRegister} />
              </label>
              <label>
                Email
                <input required type="email" name="email" value={register.email} onChange={updateRegister} autoComplete="username" />
              </label>
              <label>
                Phone
                <input required name="phone" value={register.phone} onChange={updateRegister} />
              </label>
              <label>
                Password
                <input required minLength="8" type="password" name="password" value={register.password} onChange={updateRegister} autoComplete="new-password" />
              </label>
              <label>
                Setup code
                <input required name="setupCode" value={register.setupCode} onChange={updateRegister} />
              </label>
              <label className="check-row">
                <input type="checkbox" name="repairExisting" checked={register.repairExisting} onChange={updateRegister} />
                Replace existing local admin login if setup is stuck
              </label>
              <button className="button full" type="submit">Create Admin</button>
              {setupRequired === false && (
                <button className="text-button" type="button" onClick={() => setAuthMode('login')}>Back to sign in</button>
              )}
            </form>
          ) : authMode === 'forgot' ? (
            <div className="admin-form">
              <form className="admin-form" onSubmit={requestPasswordReset}>
                <h1>Reset password</h1>
                <label>
                  Admin email
                  <input required type="email" name="email" value={forgot.email} onChange={updateForgot} autoComplete="username" />
                </label>
                <button className="button full" type="submit">Get Reset Code</button>
              </form>
              <form className="admin-form" onSubmit={submitPasswordReset}>
                <label>
                  Reset code
                  <input required name="resetToken" value={forgot.resetToken} onChange={updateForgot} />
                </label>
                <label>
                  New password
                  <input required minLength="8" type="password" name="newPassword" value={forgot.newPassword} onChange={updateForgot} autoComplete="new-password" />
                </label>
                <button className="button full" type="submit">Save New Password</button>
                <button className="text-button" type="button" onClick={() => setAuthMode('login')}>Back to sign in</button>
              </form>
            </div>
          ) : (
            <form className="admin-form" onSubmit={submitLogin}>
              <h1>Admin sign in</h1>
              <label>
                Email
                <input required type="email" name="email" value={login.email} onChange={updateLogin} autoComplete="username" />
              </label>
              <label>
                Password
                <input required type="password" name="password" value={login.password} onChange={updateLogin} autoComplete="current-password" />
              </label>
              <button className="button full" type="submit">Sign In</button>
              <button className="text-button" type="button" onClick={() => setAuthMode('forgot')}>Forgot password?</button>
            </form>
          )}
          {notice && <p className="success">{notice}</p>}
          {loginError && <p className="form-error">{loginError}</p>}
        </div>
      </section>
    );
  }

  return (
    <section className="page section admin-page">
      <div className="admin-heading">
        <div>
        <span className="eyebrow">Admin Dashboard</span>
          <h1>Manage bookings, reviews, services, and content.</h1>
        </div>
        <button className="button secondary" type="button" onClick={logout}>
          <LogOut size={18} />
          Sign Out
        </button>
      </div>

      {notice && <p className="success admin-notice">{notice}</p>}

      <div className="stats-grid">
        <article><Scissors size={20} /><strong>{services.length}</strong><span>Services</span></article>
        <article><CalendarCheck size={20} /><strong>{appointments.length}</strong><span>Bookings</span></article>
        <article><MessageSquareText size={20} /><strong>{testimonials.length}</strong><span>Reviews</span></article>
        <article><UserPlus size={20} /><strong>{adminAccounts.length}</strong><span>Admins</span></article>
      </div>

      <div className="admin-workspace">
        <section className="admin-panel">
          <div className="panel-heading">
            <h2>{editingServiceId ? 'Update Service' : 'Add Service'}</h2>
            {editingServiceId && <button className="icon-button dark" type="button" onClick={resetServiceForm} aria-label="Cancel edit"><X size={18} /></button>}
          </div>
          <form className="admin-form" onSubmit={submitService}>
            <label>
              Service name
              <input required name="serviceName" value={serviceForm.serviceName} onChange={updateServiceForm} />
            </label>
            <label>
              Category
              <input required name="category" value={serviceForm.category} onChange={updateServiceForm} />
            </label>
            <label>
              Description
              <textarea required name="description" value={serviceForm.description} onChange={updateServiceForm} rows="4" />
            </label>
            <div className="field-row">
              <label>
                Price
                <input required min="0" type="number" name="price" value={serviceForm.price} onChange={updateServiceForm} />
              </label>
              <label>
                Price label
                <input name="priceLabel" value={serviceForm.priceLabel} onChange={updateServiceForm} placeholder="From ₦5,000" />
              </label>
              <label>
                Duration
                <input required min="1" type="number" name="duration" value={serviceForm.duration} onChange={updateServiceForm} />
              </label>
            </div>
            <label>
              Image URL
              <input name="imageUrl" value={serviceForm.imageUrl} onChange={updateServiceForm} />
            </label>
            <button className="button full" type="submit">
              {editingServiceId ? <Save size={18} /> : <Plus size={18} />}
              {editingServiceId ? 'Save Service' : 'Add Service'}
            </button>
          </form>
        </section>

        <section className="admin-panel">
          <h2>Services</h2>
          <div className="admin-list">
            {services.map((service) => (
              <article key={service.serviceId} className="admin-list-item">
                <div>
                  <strong>{service.serviceName}</strong>
                  <span>{service.category} - {service.priceLabel || formatCurrency(service.price)} - {service.duration} min</span>
                </div>
                <div className="admin-actions">
                  <button className="icon-button dark" type="button" onClick={() => editService(service)} aria-label={`Edit ${service.serviceName}`}><Edit3 size={18} /></button>
                  <button className="icon-button danger" type="button" onClick={() => deleteService(service.serviceId)} aria-label={`Delete ${service.serviceName}`}><Trash2 size={18} /></button>
                </div>
              </article>
            ))}
          </div>
        </section>
      </div>

      <section className="admin-panel">
        <h2>Change Password</h2>
        <form className="admin-form password-form" onSubmit={submitChangePassword}>
          <label>
            Current password
            <input required type="password" name="currentPassword" value={changePassword.currentPassword} onChange={updateChangePassword} autoComplete="current-password" />
          </label>
          <label>
            New password
            <input required minLength="8" type="password" name="newPassword" value={changePassword.newPassword} onChange={updateChangePassword} autoComplete="new-password" />
          </label>
          <button className="button" type="submit">Update Password</button>
        </form>
      </section>

      <section className="admin-panel">
        <h2>Admin Accounts</h2>
        <form className="admin-form admin-account-form" onSubmit={submitNewAdmin}>
          <label>
            Full name
            <input required name="fullName" value={newAdmin.fullName} onChange={updateNewAdmin} />
          </label>
          <label>
            Email
            <input required type="email" name="email" value={newAdmin.email} onChange={updateNewAdmin} autoComplete="off" />
          </label>
          <label>
            Phone
            <input required name="phone" value={newAdmin.phone} onChange={updateNewAdmin} />
          </label>
          <label>
            Temporary password
            <input required minLength="8" type="password" name="password" value={newAdmin.password} onChange={updateNewAdmin} autoComplete="new-password" />
          </label>
          <button className="button" type="submit">
            <UserPlus size={18} />
            Create Admin
          </button>
        </form>
        <div className="admin-list">
          {adminAccounts.map((account) => (
            <article key={account.userId} className="admin-list-item">
              <div>
                <strong>{account.fullName}</strong>
                <span>{account.email} - {account.phone}</span>
              </div>
            </article>
          ))}
        </div>
      </section>

      <section className="admin-panel">
        <h2>Bookings</h2>
        <div className="admin-table">
          {appointments.map((appointment) => (
            <article key={appointment.appointmentId} className="admin-list-item booking-item">
              <div>
                <strong>{appointment.fullName}</strong>
                <span>{appointment.service?.serviceName ?? 'Service'} - {appointment.appointmentDate} at {appointment.appointmentTime}</span>
                <span>{appointment.email} - {appointment.phone}</span>
                {appointment.notes && <p>{appointment.notes}</p>}
              </div>
              <select value={appointment.status} onChange={(event) => setAppointmentStatus(appointment.appointmentId, event.target.value)}>
                {statusOptions.map((status) => <option key={status} value={status}>{status}</option>)}
              </select>
            </article>
          ))}
          {!appointments.length && <p className="form-note">No bookings yet.</p>}
        </div>
      </section>

      <section className="admin-panel">
        <h2>Testimonials</h2>
        <div className="admin-list">
          {testimonials.map((testimonial) => (
            <article key={testimonial.testimonialId} className="admin-list-item">
              <div>
                <strong>{testimonial.name}</strong>
                <span>{testimonial.rating}/5 - {testimonial.status}</span>
                <p>{testimonial.review}</p>
              </div>
              <div className="admin-actions">
                <button className="icon-button dark" type="button" onClick={() => setTestimonialStatus(testimonial.testimonialId, 'Approved')} aria-label="Approve review"><Check size={18} /></button>
                <button className="icon-button dark" type="button" onClick={() => setTestimonialStatus(testimonial.testimonialId, 'Rejected')} aria-label="Reject review"><X size={18} /></button>
                <button className="icon-button danger" type="button" onClick={() => deleteTestimonial(testimonial.testimonialId)} aria-label="Delete review"><Trash2 size={18} /></button>
              </div>
            </article>
          ))}
          {!testimonials.length && <p className="form-note">No reviews yet.</p>}
        </div>
      </section>
    </section>
  );
}
