let trendChart = null;

function initializeHabitCharts() {
    if (typeof Chart === 'undefined') {
        console.error('Chart.js is not loaded. Please check the script inclusion.');
        return;
    }

    const trendCanvas = document.getElementById('habitTrendChart');

    if (!trendCanvas) {
        console.error('Could not find trend chart canvas element');
        return;
    }

    const habits = getHabitsData();

    try {
        const trendData = getDailyTrendData(habits);
        const trendCtx = trendCanvas.getContext('2d');
        trendChart = new Chart(trendCtx, {
            type: 'line',
            data: {
                labels: trendData.dates,
                datasets: [
                    {
                        label: 'Total Habits Completed',
                        data: trendData.completions,
                        borderColor: '#8884d8',
                        backgroundColor: 'rgba(136, 132, 216, 0.1)',
                        tension: 0.4,
                        fill: true,
                        pointRadius: 4,
                        pointHoverRadius: 6
                    },
                    ...habits.map((habit, index) => ({
                        label: habit.name,
                        data: trendData.habitCompletions.map(dayData => dayData[index]),
                        borderColor: `hsl(${index * 60}, 70%, 50%)`,
                        backgroundColor: `hsla(${index * 60}, 70%, 50%, 0.1)`,
                        tension: 0.4,
                        fill: true,
                        pointRadius: 4,
                        pointHoverRadius: 6,
                        hidden: true
                    }))
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const count = context.raw;
                                return `${count} habit${count !== 1 ? 's' : ''} completed`;
                            }
                        }
                    },
                    legend: {
                        labels: {
                            color: 'black'
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1,
                            color: 'black'
                        },
                        grid: {
                            color: 'rgba(255, 255, 255, 0.1)'
                        }
                    },
                    x: {
                        ticks: {
                            maxRotation: 45,
                            minRotation: 45,
                            color: 'black'
                        },
                        grid: {
                            color: 'rgba(255, 255, 255, 0.1)'
                        }
                    }
                },
                animation: {
                    duration: 1000,
                    easing: 'easeOutQuad',
                    animateScale: true,
                    animateRotate: true
                },
                transitions: {
                    show: {
                        animations: {
                            opacity: {
                                from: 0,
                                to: 1,
                                duration: 500
                            }
                        }
                    },
                    hide: {
                        animations: {
                            opacity: {
                                from: 1,
                                to: 0,
                                duration: 500
                            }
                        }
                    }
                }
            }
        });
    } catch (error) {
        console.error('Error creating trend chart:', error);
    }
}

function getHabitsData() {
    return Array.from(document.querySelectorAll('.card')).filter(card => card.querySelector('.card-title'))
        .map(card => {
            const lastCompletedText = card.querySelector('.text-muted small:last-child')?.textContent;
            const completeButton = card.querySelector('.complete-habit');
            const isCompletedToday = completeButton?.classList.contains('disabled') || false;
            let lastCompletedAt = null;

            if (lastCompletedText) {
                const dateMatch = lastCompletedText.match(/Last completed: (.+)/);
                if (dateMatch) {
                    lastCompletedAt = new Date(dateMatch[1]);
                }
            }

            return {
                name: card.querySelector('.card-title')?.textContent?.trim() || 'Unnamed',
                timesComplete: 0,
                lastCompletedAt: lastCompletedAt,
                isCompletedToday: isCompletedToday
            };
        });
}

function getDailyTrendData(habits) {
    const dailyCompletions = new Map();
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    for (let i = 6; i >= 0; i--) {
        const date = new Date(today);
        date.setDate(date.getDate() - i);
        const dateKey = date.toLocaleDateString();
        dailyCompletions.set(dateKey, habits.map(() => 0));
    }

    habits.forEach((habit, habitIndex) => {
        if (habit.isCompletedToday) {
            const todayKey = today.toLocaleDateString();
            const habitData = dailyCompletions.get(todayKey);
            habitData[habitIndex] += 1;
            dailyCompletions.set(todayKey, habitData);
        }

        if (habit.lastCompletedAt) {
            const completionDate = new Date(habit.lastCompletedAt);
            completionDate.setHours(0, 0, 0, 0);

            const dateKey = completionDate.toLocaleDateString();
            if (dailyCompletions.has(dateKey)) {
                const habitData = dailyCompletions.get(dateKey);
                habitData[habitIndex] += 1;
                dailyCompletions.set(dateKey, habitData);
            }
        }
    });

    const dates = Array.from(dailyCompletions.keys());
    const habitCompletions = Array.from(dailyCompletions.values());

    const completions = habitCompletions.map(dayData =>
        dayData.reduce((total, count) => total + count, 0)
    );

    return { dates, completions, habitCompletions };
}

function updateCharts() {
    const habits = getHabitsData();
    const trendData = getDailyTrendData(habits);

    if (trendChart) {
        // Fade out current datasets with a transition
        trendChart.data.datasets.forEach((dataset, index) => {
            // Create a copy of the dataset to animate out
            const fadeOutDataset = {
                ...dataset,
                backgroundColor: index === 0
                    ? 'rgba(136, 132, 216, 0)'
                    : 'rgba(0,0,0,0)',
                borderColor: index === 0
                    ? 'rgba(136, 132, 216, 0)'
                    : 'rgba(0,0,0,0)',
                pointBackgroundColor: 'rgba(0,0,0,0)',
                pointBorderColor: 'rgba(0,0,0,0)',
            };

            // Briefly replace the dataset with the faded out version
            trendChart.data.datasets[index] = fadeOutDataset;
        });

        // Trigger a fade out animation
        trendChart.update({
            duration: 500,
            easing: 'easeOutQuad'
        });

        // After fade out, update the chart with new data
        setTimeout(() => {
            trendChart.data.labels = trendData.dates;

            // Reset the first dataset (total habits completed)
            trendChart.data.datasets[0] = {
                label: 'Total Habits Completed',
                data: trendData.completions,
                borderColor: '#8884d8',
                backgroundColor: 'rgba(136, 132, 216, 0.1)',
                tension: 0.4,
                fill: true,
                pointRadius: 4,
                pointHoverRadius: 6
            };

            // Create and add new habit datasets
            const habitDatasets = habits.map((habit, index) => ({
                label: habit.name,
                data: trendData.habitCompletions.map(dayData => dayData[index] || 0),
                borderColor: `hsl(${index * 60}, 70%, 50%)`,
                backgroundColor: `hsla(${index * 60}, 70%, 50%, 0.1)`,
                tension: 0.4,
                fill: true,
                pointRadius: 4,
                pointHoverRadius: 6,
                hidden: true
            }));

            // Remove any outdated datasets and add new ones
            trendChart.data.datasets.splice(1);
            trendChart.data.datasets.push(...habitDatasets);

            // Update with fade in animation
            trendChart.update({
                duration: 500,
                easing: 'easeOutQuad'
            });
        }, 550); // Slightly longer than the fade out duration
    }
}

document.addEventListener('DOMContentLoaded', initializeHabitCharts);

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.complete-habit').forEach(button => {
        button.addEventListener('click', async function(e) {
            const habitId = this.dataset.habitId;
            try {
                const response = await fetch('/HabitTracker/CompleteHabit', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: `id=${habitId}`
                });

                if (response.ok) {
                    const data = await response.json();
                    const card = this.closest('.card');
                    const lastCompletedElement = card.querySelector('.text-muted small:last-child');

                    if (lastCompletedElement) {
                        lastCompletedElement.textContent = `Last completed: ${data.lastCompletedAt}`;
                    }

                    this.classList.add('disabled');
                    this.disabled = true;
                    this.innerHTML = '<i class="bi bi-check-circle-fill"></i> <span>Completed</span>';

                    updateCharts();
                }
            } catch (error) {
                console.error('Error completing habit:', error);
            }
        });
    });
});