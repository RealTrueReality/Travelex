/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,html,cshtml}",
    "./wwwroot/index.html"
  ],
  darkMode: 'class',
  theme: {
    extend: {
      fontFamily: {
        sans: ['HarmonyOSSans', 'PlusJakartaSans', 'Arial', 'sans-serif']
      },
      colors: {
        canvas: 'rgb(var(--color-canvas) / <alpha-value>)',
        surface: 'rgb(var(--color-surface) / <alpha-value>)',
        'surface-muted': 'rgb(var(--color-surface-muted) / <alpha-value>)',
        content: 'rgb(var(--color-content) / <alpha-value>)',
        'content-muted': 'rgb(var(--color-content-muted) / <alpha-value>)',
        outline: 'rgb(var(--color-outline) / <alpha-value>)',
        brand: 'rgb(var(--color-brand) / <alpha-value>)',
        success: 'rgb(var(--color-success) / <alpha-value>)',
        danger: 'rgb(var(--color-danger) / <alpha-value>)',
        warning: 'rgb(var(--color-warning) / <alpha-value>)',
        primary: {
          light: 'rgb(var(--color-brand) / <alpha-value>)',
          dark: 'rgb(var(--color-brand) / <alpha-value>)'
        },
        secondary: {
          light: 'rgb(var(--color-content-muted) / <alpha-value>)',
          dark: 'rgb(var(--color-content-muted) / <alpha-value>)'
        },
        background: {
          light: 'rgb(var(--color-canvas) / <alpha-value>)',
          dark: 'rgb(var(--color-canvas) / <alpha-value>)'
        },
        card: {
          light: 'rgb(var(--color-surface) / <alpha-value>)',
          dark: 'rgb(var(--color-surface) / <alpha-value>)'
        },
        text: {
          light: 'rgb(var(--color-content) / <alpha-value>)',
          dark: 'rgb(var(--color-content) / <alpha-value>)'
        },
        subtext: {
          light: 'rgb(var(--color-content-muted) / <alpha-value>)',
          dark: 'rgb(var(--color-content-muted) / <alpha-value>)'
        },
        'input-bg': {
          light: 'rgb(var(--color-surface-muted) / <alpha-value>)',
          dark: 'rgb(var(--color-surface-muted) / <alpha-value>)'
        },
        active: {
          light: 'rgb(var(--color-surface-muted) / <alpha-value>)',
          dark: 'rgb(var(--color-surface-muted) / <alpha-value>)'
        },
        icon: {
          light: 'rgb(var(--color-surface-muted) / <alpha-value>)',
          dark: 'rgb(var(--color-surface-muted) / <alpha-value>)'
        }
      },
      borderRadius: {
        app: '1.25rem'
      }
    },
  },
  plugins: [],
}
