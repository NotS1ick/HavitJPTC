let currentChart;

async function initializeCharts() {
    try {
        const response = await fetch('/HabitTracker/GetHabitChartData');
        const chartData = await response.json();
        
        createGoalsChart(chartData);
        
        setupChartDropdown(chartData);
    } catch (error) {
        console.error('Error initializing charts:', error);
    }
}

function setupChartDropdown(chartData) {
    const dropdownItems = document.querySelectorAll('#statistics .dropdown-menu .dropdown-item');
    dropdownItems.forEach(item => {
        item.addEventListener('click', function(e) {
            e.preventDefault();
            const chartType = this.getAttribute('data-chart-type');
            updateActiveChart(chartType, chartData);
            
            const dropdownButton = document.getElementById('dropdownMenuBtn');
            dropdownButton.textContent = this.textContent;
        });
    });
}

function createGoalsChart(chartData) {
    if (currentChart) {
        currentChart.destroy();
    }

    const ctx = document.getElementById('goalProgressChart').getContext('2d');
    currentChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: chartData.goalsLabels,
            datasets: [{
                label: 'Goal Progress',
                data: chartData.goalsData,
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1,
                borderRadius: 4
            }]
        },
        options: getDefaultChartOptions('Goal Progress')
    });
}

function createCompletionsChart(chartData) {
    if (currentChart) {
        currentChart.destroy();
    }

    const ctx = document.getElementById('goalProgressChart').getContext('2d');
    currentChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: chartData.completionLabels,
            datasets: [{
                label: 'Times Completed',
                data: chartData.completionData,
                backgroundColor: 'rgba(54, 162, 235, 0.2)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1,
                borderRadius: 4
            }]
        },
        options: getDefaultChartOptions('Habit Completions')
    });
}

function getDefaultChartOptions(title) {
    return {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            title: {
                display: true,
                text: title,
                font: {
                    size: 16
                }
            },
            legend: {
                position: 'top',
                labels: {
                    padding: 10,
                    font: {
                        size: 12
                    }
                }
            }
        },
        scales: {
            x: {
                grid: {
                    drawBorder: false,
                    color: 'rgba(200, 200, 200, 0.1)'
                },
                ticks: {
                    font: {
                        size: 11
                    },
                    maxRotation: 45,
                    minRotation: 45
                }
            },
            y: {
                grid: {
                    drawBorder: false,
                    color: 'rgba(200, 200, 200, 0.1)'
                },
                beginAtZero: true,
                ticks: {
                    stepSize: 1,
                    font: {
                        size: 11
                    }
                }
            }
        }
    };
}

function getRandomColor() {
    const letters = '0123456789ABCDEF';
    let color = '#';
    for (let i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

function updateActiveChart(chartType, chartData) {
    switch(chartType) {
        case 'goals':
            createGoalsChart(chartData);
            break;
        case 'completions':
            createCompletionsChart(chartData);
            break;
    }
}

async function refreshAllStats() {
    try {
        const response = await fetch('/HabitTracker/GetHabitChartData');
        if (!response.ok) {
            throw new Error('Failed to fetch chart data');
        }
        const chartData = await response.json();
        
        const dropdownButton = document.getElementById('dropdownMenuBtn');
        const activeItem = document.querySelector('.dropdown-item.active');
        if (activeItem) {
            updateActiveChart(activeItem.getAttribute('data-chart-type'), chartData);
        } else {
            createGoalsChart(chartData);
        }
    } catch (error) {
        console.error('Error updating charts:', error);
    }
}

document.addEventListener('DOMContentLoaded', initializeCharts);

window.refreshAllStats = refreshAllStats;