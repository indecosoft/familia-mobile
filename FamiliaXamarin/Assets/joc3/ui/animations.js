$.keyframe.define([{
        name: 'showBox',
        '0%': {
            transform: 'translateY(-150%);'
        },
        '100%': {
            transition: '0.5s;',
            transform: 'translateY(0%);'
        }
    },
    {
        name: 'byeBox',
        '0%': {
            transform: 'translateY(0%);'
        },
        '100%': {
            transition: '0.5s;',
            transform: 'translateY(200%);',
            'color': 'rgba(0,0,0,0.0)'
        }
    }, {
        name: 'moveUp',
        '100%': {
            transition: '0.5s;',
            transform: 'translateY(-40%);',
            'font-size': '34px',
        }
    }
]);