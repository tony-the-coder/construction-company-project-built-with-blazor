/** @type {import('tailwindcss').Config} */
const defaultTheme = require('tailwindcss/defaultTheme'); // Import defaultTheme for font fallbacks

module.exports = {
    // Keep your existing content paths, ensuring they cover all component/page locations
    content: [
        './Components/**/*.razor', // Check if Components folder exists at root, adjust path if needed
        './Pages/**/*.razor',     // Check if Pages folder exists at root, adjust path if needed
        './Layout/**/*.razor',    // Check if Layout folder exists at root, adjust path if needed
        './**/*.razor',           // Add a broader pattern just in case
        './wwwroot/index.html',   // Keep if relevant (usually for static fallback or initial load)
        './node_modules/flowbite/**/*.js' // Required for Flowbite JS components if using them
    ],
    theme: {
        extend: {
            colors: {
                // Define the refined color palette
                'background': '#E9E4DE', // Main page background (Off-White)
                'primary': '#14293A',   // Main text, dark elements (Deep Navy)
                'secondary': '#6B7280', // Lighter text (like Tailwind gray-500)
                'accent': '#AE8F69',     // Muted Gold/Bronze for highlights
                'subtle': '#D1D5DB'      // Light gray for borders (like Tailwind gray-300)
            },
            fontFamily: {
                // Set 'sans' as the default body font using Open Sans
                // It inherits the default sans stack (...defaultTheme.fontFamily.sans) for fallbacks
                'sans': ['Open Sans', ...defaultTheme.fontFamily.sans],
                // Set 'serif' for headings using Playfair Display
                // It inherits the default serif stack (...defaultTheme.fontFamily.serif) for fallbacks
                'serif': ['Playfair Display', ...defaultTheme.fontFamily.serif],
            },
        },
    },
    plugins: [
        require("flowbite/plugin"), // Keep the Flowbite plugin
        // Add other Tailwind plugins if needed later (e.g., forms, typography)
        // require('@tailwindcss/forms'),
        // require('@tailwindcss/typography'),
        // require('@tailwindcss/line-clamp'), // If needed for line clamping
    ],
};