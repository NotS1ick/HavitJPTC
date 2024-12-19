class TypeWriter {
    constructor(element, options = {}) {
        this.element = element;
        this.words = element.textContent.trim().split(/\s+/);
        this.element.setAttribute('data-final-text', this.words.join(' '));
        this.element.textContent = '';
        this.delay = options.delay || .5;
        this.currentWord = 0;
        this.currentChar = 0;
        this.onComplete = options.onComplete || (() => {});

        if (!this.element.querySelector('.typewriter-wrapper')) {
            const wrapper = document.createElement('span');
            wrapper.className = 'typewriter-wrapper';
            this.element.appendChild(wrapper);
        }
        this.wrapper = this.element.querySelector('.typewriter-wrapper');

        // Create a container for the current word
        this.currentWordSpan = document.createElement('span');
        this.currentWordSpan.className = 'typewriter-word';
        this.wrapper.appendChild(this.currentWordSpan);
    }

    type() {
        if (this.currentWord < this.words.length) {
            const word = this.words[this.currentWord];

            if (this.currentChar === 0) {
                // Start a new word
                this.currentWordSpan = document.createElement('span');
                this.currentWordSpan.className = 'typewriter-word';
                this.wrapper.appendChild(this.currentWordSpan);

                // Add space after previous word (except for first word)
                if (this.currentWord > 0) {
                    const space = document.createElement('span');
                    space.className = 'typewriter-space';
                    space.innerHTML = '&nbsp;';
                    this.wrapper.insertBefore(space, this.currentWordSpan);
                }
            }

            if (this.currentChar < word.length) {
                const char = word[this.currentChar];
                const span = document.createElement('span');
                span.className = 'typewriter-char';
                span.textContent = char;
                this.currentWordSpan.appendChild(span);

                void span.offsetWidth; // Force reflow
                span.classList.add('visible');

                this.currentChar++;
                setTimeout(() => this.type(), this.delay);
            } else {
                // Word complete, move to next word
                this.currentWord++;
                this.currentChar = 0;
                setTimeout(() => this.type(), this.delay);
            }
        } else {
            this.onComplete();
        }
    }
}

function initTypewriterOnScroll() {
    const textElements = document.querySelectorAll('.typewriter');

    // Initially hide all typewriter buttons
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
                        delay: parseInt(entry.target.dataset.delay) || .5,
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
                        delay: parseInt(entry.target.dataset.delay) || .5
                    });
                    typewriter.type();
                }
            }
        });
    }, options);

    textElements.forEach(element => observer.observe(element));
}

document.addEventListener('DOMContentLoaded', initTypewriterOnScroll);