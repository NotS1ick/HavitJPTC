class TypeWriter {
    constructor(element, options = {}) {
        this.element = element;
        this.originalText = element.textContent;
        this.element.setAttribute('data-final-text', this.originalText);
        this.element.textContent = '';
        this.delay = options.delay || 50;
        this.currentChar = 0;
        this.onComplete = options.onComplete || (() => {});

        if (!this.element.querySelector('.typewriter-wrapper')) {
            const wrapper = document.createElement('span');
            wrapper.className = 'typewriter-wrapper';
            this.element.appendChild(wrapper);
        }
        this.wrapper = this.element.querySelector('.typewriter-wrapper');
    }

    type() {
        if (this.currentChar < this.originalText.length) {
            const char = this.originalText.charAt(this.currentChar);
            const span = document.createElement('span');

            if (char === ' ') {
                span.className = 'typewriter-space';
                span.innerHTML = '&nbsp;';
            } else {
                span.className = 'typewriter-char';
                span.textContent = char;
            }

            this.wrapper.appendChild(span);

            void span.offsetWidth;
            span.classList.add('visible');

            this.currentChar++;
            setTimeout(() => this.type(), this.delay);
        } else {
            this.onComplete();
        }
    }
}

function initTypewriterOnScroll() {
    const textElements = document.querySelectorAll('.typewriter');
    
    document.querySelectorAll('.typewriter.btn').forEach(btn => {
        btn.style.opacity = '0';
        btn.style.visibility = 'hidden';
        btn.style.transition = 'none';
    });

    const options = {
        root: null,
        rootMargin: '0px',
        threshold: 0.5
    };

    const observer = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting && !entry.target.dataset.typed) {
                entry.target.dataset.typed = 'true';
                
                if (entry.target.classList.contains('btn')) {
                    return;
                }
                
                const section = entry.target.closest('section');
                const button = section?.querySelector('.btn.typewriter');
                const paragraphs = section?.querySelectorAll('p.typewriter');
                
                if (entry.target.tagName === 'P' && button &&
                    Array.from(paragraphs).indexOf(entry.target) === paragraphs.length - 1) {

                    const typewriter = new TypeWriter(entry.target, {
                        delay: parseInt(entry.target.dataset.delay) || 50,
                        onComplete: () => {
                            setTimeout(() => {
                                button.style.transition = 'opacity 0.5s ease-in, transform 0.5s ease-in';
                                button.style.visibility = 'visible';
                                button.style.transform = 'translateY(20px)';
                                
                                void button.offsetWidth;

                                button.style.opacity = '1';
                                button.style.transform = 'translateY(0)';
                            }, 200);
                        }
                    });
                    typewriter.type();
                } else {
                    const typewriter = new TypeWriter(entry.target, {
                        delay: parseInt(entry.target.dataset.delay) || 50
                    });
                    typewriter.type();
                }
            }
        });
    }, options);

    textElements.forEach(element => observer.observe(element));
}

document.addEventListener('DOMContentLoaded', initTypewriterOnScroll);