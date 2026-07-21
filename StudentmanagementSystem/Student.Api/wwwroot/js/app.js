// Student Management System - Client Application JavaScript

document.addEventListener('DOMContentLoaded', () => {
    // API Configuration
    const API_BASE = '/api';

    // State
    let jwtToken = localStorage.getItem('jwt_token') || null;
    let currentUser = JSON.parse(localStorage.getItem('current_user') || 'null');
    let studentsCache = [];
    let studentToDeleteId = null;

    // DOM Elements
    const authBtn = document.getElementById('auth-btn');
    const authStatusBadge = document.getElementById('auth-status-badge');
    const statusText = document.getElementById('status-text');
    const userInfo = document.getElementById('user-info');
    const loggedUserName = document.getElementById('logged-user-name');
    const loggedUserRole = document.getElementById('logged-user-role');

    // Metrics
    const metricTotalStudents = document.getElementById('metric-total-students');
    const metricTotalCourses = document.getElementById('metric-total-courses');
    const metricAvgAge = document.getElementById('metric-avg-age');

    // Controls
    const searchInput = document.getElementById('search-input');
    const courseFilter = document.getElementById('course-filter');
    const refreshBtn = document.getElementById('refresh-btn');
    const addStudentBtn = document.getElementById('add-student-btn');

    // Table
    const studentTableBody = document.getElementById('student-table-body');
    const tableLoading = document.getElementById('table-loading');
    const tableEmpty = document.getElementById('table-empty');

    // Auth Modal
    const authModal = document.getElementById('auth-modal');
    const tabLogin = document.getElementById('tab-login');
    const tabRegister = document.getElementById('tab-register');
    const loginForm = document.getElementById('login-form');
    const registerForm = document.getElementById('register-form');

    // Student Modal
    const studentModal = document.getElementById('student-modal');
    const studentForm = document.getElementById('student-form');
    const studentModalTitle = document.getElementById('student-modal-title');
    const studentIdInput = document.getElementById('student-id');
    const studentNameInput = document.getElementById('student-name');
    const studentEmailInput = document.getElementById('student-email');
    const studentAgeInput = document.getElementById('student-age');
    const studentCourseInput = document.getElementById('student-course');

    // Delete Modal
    const deleteModal = document.getElementById('delete-modal');
    const deleteStudentName = document.getElementById('delete-student-name');
    const confirmDeleteBtn = document.getElementById('confirm-delete-btn');

    // Initialize Application
    init();

    function init() {
        updateAuthUI();
        bindEvents();
        fetchStudents();
    }

    function updateAuthUI() {
        if (jwtToken && currentUser) {
            authStatusBadge.className = 'status-badge logged-in';
            statusText.textContent = 'Authenticated';
            userInfo.classList.remove('hidden');
            loggedUserName.textContent = currentUser.username || 'User';
            loggedUserRole.textContent = currentUser.role || 'User';
            authBtn.innerHTML = `<i class="fa-solid fa-right-from-bracket"></i><span>Logout</span>`;
        } else {
            authStatusBadge.className = 'status-badge logged-out';
            statusText.textContent = 'Guest Mode';
            userInfo.classList.add('hidden');
            authBtn.innerHTML = `<i class="fa-solid fa-right-to-bracket"></i><span>Login</span>`;
        }
    }

    function bindEvents() {
        // Auth Toggle
        authBtn.addEventListener('click', () => {
            if (jwtToken) {
                logout();
            } else {
                openModal(authModal);
            }
        });

        // Tabs
        tabLogin.addEventListener('click', () => {
            tabLogin.classList.add('active');
            tabRegister.classList.remove('active');
            loginForm.classList.remove('hidden');
            registerForm.classList.add('hidden');
        });

        tabRegister.addEventListener('click', () => {
            tabRegister.classList.add('active');
            tabLogin.classList.remove('active');
            registerForm.classList.remove('hidden');
            loginForm.classList.add('hidden');
        });

        // Forms
        loginForm.addEventListener('submit', handleLogin);
        registerForm.addEventListener('submit', handleRegister);
        studentForm.addEventListener('submit', handleSaveStudent);
        confirmDeleteBtn.addEventListener('click', handleConfirmDelete);

        // Search & Filters
        let searchTimeout;
        searchInput.addEventListener('input', () => {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(fetchStudents, 300);
        });

        courseFilter.addEventListener('change', fetchStudents);
        refreshBtn.addEventListener('click', fetchStudents);

        addStudentBtn.addEventListener('click', () => {
            if (!jwtToken) {
                showToast('Please login first to add students.', 'error');
                openModal(authModal);
                return;
            }
            openStudentModal();
        });

        // Modal Close Listeners
        document.querySelectorAll('.close-modal-btn, .cancel-modal-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const backdrop = e.target.closest('.modal-backdrop');
                if (backdrop) closeModal(backdrop);
            });
        });
    }

    // API Helper with JWT Header
    async function apiRequest(url, method = 'GET', body = null) {
        const headers = { 'Content-Type': 'application/json' };
        if (jwtToken) {
            headers['Authorization'] = `Bearer ${jwtToken}`;
        }

        const options = { method, headers };
        if (body) {
            options.body = JSON.stringify(body);
        }

        try {
            const response = await fetch(url, options);
            const data = await response.json();

            if (!response.ok) {
                if (response.status === 401) {
                    if (jwtToken) {
                        showToast('Session expired. Please login again.', 'error');
                        logout();
                    }
                }
                const errorMessage = data.message || (data.errors && data.errors.join(', ')) || 'API request failed';
                throw new Error(errorMessage);
            }

            return data;
        } catch (err) {
            throw err;
        }
    }

    // Fetch Students
    async function fetchStudents() {
        showLoading(true);
        try {
            const search = searchInput.value.trim();
            const course = courseFilter.value;

            let query = `${API_BASE}/students?`;
            if (search) query += `searchTerm=${encodeURIComponent(search)}&`;
            if (course) query += `course=${encodeURIComponent(course)}`;

            const response = await apiRequest(query);
            studentsCache = response.data || [];
            
            updateCourseFilterOptions();
            renderTable(studentsCache);
            updateMetrics(studentsCache);
        } catch (err) {
            console.warn('Fetch students notice:', err.message);
            if (!jwtToken) {
                tableEmpty.innerHTML = `
                    <i class="fa-solid fa-lock empty-icon"></i>
                    <h3>Authentication Required</h3>
                    <p>Protected API Endpoint. Click <strong>Login</strong> above to authenticate.</p>
                `;
            } else {
                tableEmpty.innerHTML = `
                    <i class="fa-solid fa-triangle-exclamation empty-icon"></i>
                    <h3>Error Loading Students</h3>
                    <p>${err.message}</p>
                `;
            }
            studentTableBody.innerHTML = '';
            tableLoading.classList.add('hidden');
            tableEmpty.classList.remove('hidden');
        }
    }

    function renderTable(students) {
        tableLoading.classList.add('hidden');

        if (!students || students.length === 0) {
            studentTableBody.innerHTML = '';
            tableEmpty.classList.remove('hidden');
            return;
        }

        tableEmpty.classList.add('hidden');
        studentTableBody.innerHTML = students.map(student => {
            const initials = getInitials(student.name);
            const formattedDate = new Date(student.createdDate).toLocaleDateString('en-US', {
                year: 'numeric', month: 'short', day: 'numeric'
            });

            return `
                <tr>
                    <td>#${student.id}</td>
                    <td>
                        <div class="student-info-cell">
                            <div class="avatar-badge">${initials}</div>
                            <div class="student-details">
                                <div class="name">${escapeHtml(student.name)}</div>
                                <div class="email">${escapeHtml(student.email)}</div>
                            </div>
                        </div>
                    </td>
                    <td><strong>${student.age}</strong> yrs</td>
                    <td><span class="course-pill">${escapeHtml(student.course)}</span></td>
                    <td style="color: var(--text-muted); font-size: 0.85rem;">${formattedDate}</td>
                    <td class="text-right">
                        <div class="action-btn-group">
                            <button class="action-btn edit" onclick="window.editStudent(${student.id})" title="Edit Student">
                                <i class="fa-solid fa-pen"></i>
                            </button>
                            <button class="action-btn delete" onclick="window.confirmDeleteStudent(${student.id}, '${escapeHtml(student.name)}')" title="Delete Student">
                                <i class="fa-solid fa-trash-can"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        }).join('');
    }

    function updateMetrics(students) {
        const total = students.length;
        const courses = new Set(students.map(s => s.course)).size;
        const avgAge = total > 0 ? (students.reduce((acc, s) => acc + s.age, 0) / total).toFixed(1) : '0.0';

        metricTotalStudents.textContent = total;
        metricTotalCourses.textContent = courses;
        metricAvgAge.textContent = avgAge;
    }

    function updateCourseFilterOptions() {
        const currentSelected = courseFilter.value;
        const courses = Array.from(new Set(studentsCache.map(s => s.course))).sort();

        courseFilter.innerHTML = '<option value="">All Courses</option>' + 
            courses.map(c => `<option value="${escapeHtml(c)}" ${c === currentSelected ? 'selected' : ''}>${escapeHtml(c)}</option>`).join('');
    }

    // Login Handler
    async function handleLogin(e) {
        e.preventDefault();
        const usernameOrEmail = document.getElementById('login-username').value.trim();
        const password = document.getElementById('login-password').value;

        try {
            const res = await apiRequest(`${API_BASE}/auth/login`, 'POST', { usernameOrEmail, password });
            jwtToken = res.data.token;
            currentUser = { username: res.data.username, email: res.data.email, role: res.data.role };

            localStorage.setItem('jwt_token', jwtToken);
            localStorage.setItem('current_user', JSON.stringify(currentUser));

            updateAuthUI();
            closeModal(authModal);
            showToast(`Welcome back, ${currentUser.username}!`, 'success');
            fetchStudents();
        } catch (err) {
            showToast(err.message, 'error');
        }
    }

    // Register Handler
    async function handleRegister(e) {
        e.preventDefault();
        const username = document.getElementById('reg-username').value.trim();
        const email = document.getElementById('reg-email').value.trim();
        const password = document.getElementById('reg-password').value;

        try {
            const res = await apiRequest(`${API_BASE}/auth/register`, 'POST', { username, email, password, role: 'User' });
            jwtToken = res.data.token;
            currentUser = { username: res.data.username, email: res.data.email, role: res.data.role };

            localStorage.setItem('jwt_token', jwtToken);
            localStorage.setItem('current_user', JSON.stringify(currentUser));

            updateAuthUI();
            closeModal(authModal);
            showToast(`Account created successfully! Welcome, ${currentUser.username}.`, 'success');
            fetchStudents();
        } catch (err) {
            showToast(err.message, 'error');
        }
    }

    // Save Student (Add / Edit)
    async function handleSaveStudent(e) {
        e.preventDefault();
        const id = studentIdInput.value;
        const name = studentNameInput.value.trim();
        const email = studentEmailInput.value.trim();
        const age = parseInt(studentAgeInput.value, 10);
        const course = studentCourseInput.value.trim();

        const payload = { name, email, age, course };

        try {
            if (id) {
                await apiRequest(`${API_BASE}/students/${id}`, 'PUT', payload);
                showToast('Student updated successfully!', 'success');
            } else {
                await apiRequest(`${API_BASE}/students`, 'POST', payload);
                showToast('Student added successfully!', 'success');
            }
            closeModal(studentModal);
            fetchStudents();
        } catch (err) {
            showToast(err.message, 'error');
        }
    }

    // Edit Modal Helper
    window.editStudent = (id) => {
        const student = studentsCache.find(s => s.id === id);
        if (!student) return;

        studentIdInput.value = student.id;
        studentNameInput.value = student.name;
        studentEmailInput.value = student.email;
        studentAgeInput.value = student.age;
        studentCourseInput.value = student.course;

        studentModalTitle.textContent = 'Edit Student Record';
        document.getElementById('student-submit-label').textContent = 'Update Student';
        openModal(studentModal);
    };

    // Confirm Delete Helper
    window.confirmDeleteStudent = (id, name) => {
        if (!jwtToken) {
            showToast('Please login to delete students.', 'error');
            openModal(authModal);
            return;
        }
        studentToDeleteId = id;
        deleteStudentName.textContent = name;
        openModal(deleteModal);
    };

    async function handleConfirmDelete() {
        if (!studentToDeleteId) return;

        try {
            await apiRequest(`${API_BASE}/students/${studentToDeleteId}`, 'DELETE');
            showToast('Student record deleted.', 'success');
            closeModal(deleteModal);
            studentToDeleteId = null;
            fetchStudents();
        } catch (err) {
            showToast(err.message, 'error');
        }
    }

    function openStudentModal() {
        studentForm.reset();
        studentIdInput.value = '';
        studentModalTitle.textContent = 'Add New Student';
        document.getElementById('student-submit-label').textContent = 'Save Student';
        openModal(studentModal);
    }

    function logout() {
        jwtToken = null;
        currentUser = null;
        localStorage.removeItem('jwt_token');
        localStorage.removeItem('current_user');
        updateAuthUI();
        showToast('Logged out successfully.', 'success');
        fetchStudents();
    }

    // Modal Helpers
    function openModal(modal) { modal.classList.remove('hidden'); }
    function closeModal(modal) { modal.classList.add('hidden'); }
    function showLoading(show) {
        if (show) {
            tableLoading.classList.remove('hidden');
            tableEmpty.classList.add('hidden');
        } else {
            tableLoading.classList.add('hidden');
        }
    }

    // Utilities
    function getInitials(name) {
        if (!name) return 'S';
        const parts = name.trim().split(' ');
        if (parts.length >= 2) return (parts[0][0] + parts[1][0]).toUpperCase();
        return parts[0][0].toUpperCase();
    }

    function escapeHtml(str) {
        if (!str) return '';
        return str.replace(/[&<>"']/g, match => {
            const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
            return map[match];
        });
    }

    function showToast(message, type = 'success') {
        const container = document.getElementById('toast-container');
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.innerHTML = `
            <i class="fa-solid ${type === 'success' ? 'fa-circle-check' : 'fa-circle-exclamation'}"></i>
            <span>${escapeHtml(message)}</span>
        `;
        container.appendChild(toast);
        setTimeout(() => {
            toast.style.animation = 'toastIn 0.3s ease reverse forwards';
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    }
});
