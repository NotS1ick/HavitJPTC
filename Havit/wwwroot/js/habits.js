document.addEventListener('DOMContentLoaded', function () {
    const editHabitModal = document.getElementById('editHabitModal');
    const imagePreview = document.getElementById('imagePreview');
    const imageFile = document.getElementById('imageFile');
    
    function storeScrollPosition() {
        sessionStorage.setItem('scrollPosition', window.scrollY);
    }
    
    function restoreScrollPosition() {
        const scrollPosition = sessionStorage.getItem('scrollPosition');
        if (scrollPosition) {
            window.scrollTo(0, parseInt(scrollPosition));
            sessionStorage.removeItem('scrollPosition');
        }
    }

    editHabitModal.addEventListener('show.bs.modal', function (event) {
        const button = event.relatedTarget;
        const habitId = button.getAttribute('data-habit-id');
        const habitName = button.getAttribute('data-habit-name');
        const habitFrequency = button.getAttribute('data-habit-frequency');

        const modal = this;
        modal.querySelector('#habitId').value = habitId;
        modal.querySelector('#habitName').value = habitName;
        modal.querySelector('#frequency').value = habitFrequency;

        imagePreview.classList.add('d-none');
        imagePreview.src = '';
    });

    const newImagePreview = document.getElementById('newImagePreview');
    const newImageFile = document.getElementById('newImageFile');

    function handleImagePreview(file, previewElement) {
        if (file) {
            if (file.size > 5 * 1024 * 1024) {
                alert('File size must be less than 5MB');
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                previewElement.src = e.target.result;
                previewElement.classList.remove('d-none');
            };
            reader.readAsDataURL(file);
        }
    }

    imageFile.addEventListener('change', (event) => {
        handleImagePreview(event.target.files[0], imagePreview);
    });

    newImageFile.addEventListener('change', (event) => {
        handleImagePreview(event.target.files[0], newImagePreview);
    });

    document.querySelectorAll('.complete-habit').forEach(button => {
        button.addEventListener('click', async function(e) {
            e.preventDefault();
            if (this.classList.contains('disabled')) return;

            const habitId = this.getAttribute('data-habit-id');

            try {
                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                if (!tokenElement) {
                    throw new Error('Anti-forgery token not found');
                }

                const formData = new FormData();
                formData.append('id', habitId);
                formData.append('__RequestVerificationToken', tokenElement.value);

                this.classList.add('disabled');
                this.setAttribute('disabled', '');

                const response = await fetch('/HabitTracker/CompleteHabit', {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) {
                    throw new Error('Failed to complete habit');
                }
                const result = await response.json();
                
                showToast(`Habit completed successfully!`);

            } catch (error) {
                console.error('Error completing habit:', error);
                alert('Failed to complete habit. Please try again.');
                this.classList.remove('disabled');
                this.removeAttribute('disabled');
            }
        });
    });

    function showToast(message) {
        const toastContainer = document.getElementById('toastContainer');

        const toast = document.createElement('div');
        toast.className = 'toast align-items-center text-bg-success border-0';
        toast.role = 'alert';
        toast.ariaLive = 'assertive';
        toast.ariaAtomic = 'true';
        toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

        toastContainer.appendChild(toast);
        
        const bootstrapToast = new bootstrap.Toast(toast);
        bootstrapToast.show();
        
        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }
    
    window.addEventListener('load', restoreScrollPosition);

    document.querySelectorAll('.delete-habit').forEach(button => {
        button.addEventListener('click', async function () {
            const habitId = this.getAttribute('data-habit-id');
            if (confirm('Are you sure you want to delete this habit?')) {
                try {
                    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                    if (!tokenElement) {
                        throw new Error('Anti-forgery token not found');
                    }

                    const formData = new FormData();
                    formData.append('id', habitId);
                    formData.append('__RequestVerificationToken', tokenElement.value);
                    
                    storeScrollPosition();

                    const response = await fetch('/HabitTracker/DeleteHabit', {
                        method: 'POST',
                        body: formData
                    });

                    if (!response.ok) {
                        throw new Error('Failed to delete habit');
                    }

                    window.location.reload();
                } catch (error) {
                    console.error('Error deleting habit:', error);
                    alert('Failed to delete habit. Please try again.');
                }
            }
        });
    });
});