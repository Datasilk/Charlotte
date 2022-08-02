'use strict';

//includes
var gulp = require('gulp'),
    less = require('gulp-less');

//paths
var paths = {
    css: 'CSS/',
    webroot: 'wwwroot/',
};

//working paths
paths.working = {
    less: {
        site: paths.css + 'site.less'
    }
};

//compiled paths
paths.compiled = {
    css: paths.webroot + 'css/',
};
//tasks for compiling LESS & CSS /////////////////////////////////////////////////////////////////////
gulp.task('less:site', function () {
    var p = gulp.src(paths.working.less.site)
        .pipe(less());
    return p.pipe(gulp.dest(paths.compiled.css, { overwrite: true }));
});

gulp.task('less', gulp.series('less:site'));

//default task ////////////////////////////////////////////////////////////////////////////
gulp.task('default', gulp.series('less'));

//watch task /////////////////////////////////////////////////////////////////////
gulp.task('watch', function () {
    //watch site LESS
    gulp.watch([
        paths.working.less.site,
        paths.css + 'themes/default.less'
    ], gulp.series('less:site'));
});