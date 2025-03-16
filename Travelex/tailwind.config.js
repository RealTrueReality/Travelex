/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,html,cshtml}",
    "./Pages/**/*.{razor,html,cshtml}",
    "./wwwroot/index.html"
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        primary: {
          light: '#0085FF',
          dark: '#1E90FF'
        },
        secondary: {
          light: '#4C7C99',
          dark: '#6A98B5'
        },
        background: {
          light: '#FFFFFF',
          dark: '#121212'
        },
        card: {
          light: '#FFFFFF',
          dark: '#1E1E1E'
        },
        text: {
          light: '#1C160C',
          dark: '#E0E0E0'
        },
        subtext: {
          light: '#666666',
          dark: '#A0A0A0'
        }
      }
    },
  },
  plugins: [],
}
