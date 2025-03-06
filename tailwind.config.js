﻿/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './Components/**/*.{razor,html,cs}',
        './Pages/**/*.{razor,html,cs}',
        './Layout/**/*.{razor,html,cs}',
        './wwwroot/index.html',
        "./node_modules/flowbite/**/*.js"
    ],
    plugins: [
        require('flowbite/plugin')
    ],
};