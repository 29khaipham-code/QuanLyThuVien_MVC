(() => {
  const scene = document.getElementById('authScene');
  if (!scene) return;
  const lamp = document.getElementById('lampToggle');
  const signInTab = document.getElementById('signInTab');
  const signUpTab = document.getElementById('signUpTab');
  const signInForm = document.getElementById('signInForm');
  const signUpForm = document.getElementById('signUpForm');
  const title = document.getElementById('formTitle');
  const subtitle = document.getElementById('formSubtitle');
  let isOn = scene.dataset.errors === 'true';
  let mode = 'signin';
  function syncLamp() {
    scene.classList.toggle('is-off', !isOn);
    lamp.setAttribute('aria-pressed', String(isOn));
    lamp.setAttribute('aria-label', isOn ? 'Tắt robot và ẩn biểu mẫu' : 'Bật robot và hiện biểu mẫu');
    document.getElementById('authPanel').setAttribute('aria-hidden', String(!isOn));
    if ('inert' in document.getElementById('authPanel')) document.getElementById('authPanel').inert = !isOn;
  }
  function setMode(next) {
    mode = next;
    isOn = true;
    scene.classList.toggle('theme-green', mode === 'signup');
    scene.classList.toggle('theme-red', mode !== 'signup');
    const signUp = mode === 'signup';
    signInTab.classList.toggle('active', !signUp);
    signUpTab.classList.toggle('active', signUp);
    signInTab.setAttribute('aria-selected', String(!signUp));
    signUpTab.setAttribute('aria-selected', String(signUp));
    signInForm.hidden = signUp;
    signUpForm.hidden = !signUp;
    title.textContent = signUp ? 'Create Account' : 'Welcome Back';
    subtitle.textContent = signUp ? 'Tham gia hệ thống quản lý thư viện' : 'Đăng nhập vào hệ thống thư viện';
    syncLamp();
  }
  lamp.addEventListener('click', () => { isOn = !isOn; syncLamp(); });
  signInTab.addEventListener('click', () => setMode('signin'));
  signUpTab.addEventListener('click', () => setMode('signup'));
  document.querySelectorAll('[data-switch]').forEach(el => el.addEventListener('click', () => setMode(el.dataset.switch)));
  document.querySelectorAll('.eye-toggle').forEach(button => button.addEventListener('click', () => {
    const input = document.getElementById(button.dataset.target);
    if (!input) return;
    input.type = input.type === 'password' ? 'text' : 'password';
    button.setAttribute('aria-label', input.type === 'password' ? 'Hiện mật khẩu' : 'Ẩn mật khẩu');
  }));
  syncLamp();
})();
