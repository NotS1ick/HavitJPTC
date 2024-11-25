document.addEventListener('DOMContentLoaded', function () {
    const editHabitModal = document.getElementById('editHabitModal');
    const imagePreview = document.getElementById('imagePreview');
    const imageFile = document.getElementById('imageFile');

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
        button.addEventListener('click', async function () {
            if (this.classList.contains('disabled')) return;

            const habitId = this.getAttribute('data-habit-id');
            const button = this;
    
            try {
                const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                if (!tokenElement) {
                    throw new Error('Anti-forgery token not found');
                }

                const formData = new FormData();
                formData.append('id', habitId);
                formData.append('__RequestVerificationToken', tokenElement.value);

                const response = await fetch('/HabitTracker/CompleteHabit', {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) {
                    throw new Error('Failed to complete habit');
                }

                const result = await response.json();

                const card = button.closest('.card');
                card.classList.add('opacity-75');
                button.classList.add('disabled');
                button.setAttribute('disabled', '');
                button.innerHTML = '<i class="bi bi-check-circle-fill"></i> <span>Completed</span>';

                const streakElement = card.querySelector('.card-footer small');
                streakElement.textContent = `Current streak: ${result.timesComplete} days`;

            } catch (error) {
                console.error('Error completing habit:', error);
                alert('Failed to complete habit. Please try again.');
            }
        });
    });

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
