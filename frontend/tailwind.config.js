/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        fiscal: {
          navy: '#0B326F',
          blue: '#008BD2',
          cyan: '#24B4E8',
          ink: '#172033',
          mist: '#EEF7FB'
        }
      },
      boxShadow: {
        panel: '0 14px 36px rgba(11, 50, 111, 0.10)'
      }
    }
  },
  plugins: []
};
