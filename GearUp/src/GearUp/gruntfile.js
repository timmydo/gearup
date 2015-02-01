// This file in the main entry point for defining grunt tasks and using grunt plugins.
// Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409

module.exports = function (grunt) {
    grunt.initConfig({
        typescript: {
            base: {
                src: ['app/**/*.ts'],
                dest: 'wwwroot/js',
                options: {
                    module: 'amd', //or commonjs 
                    target: 'es5', //or es3 
                    basePath: 'app',
                    sourceMap: true,
                    declaration: true
                }
            }
        },
        tsd: {
            refresh: {
                options: {
                    // execute a command
                    command: 'reinstall',

                    //optional: always get from HEAD
                    latest: true,

                    // specify config file
                    config: 'tsd.json',

                    // experimental: options to pass to tsd.API
                    opts: {
                        // props from tsd.Options
                    }
                }
            }
        },
        cssmin: {  
            sitecss: {  
                options: {  
                    banner: '/* My minified css file */'  
                },  
                files: {  
                    'wwwroot/css/site.min.css': [  
                        'css/site.css']  
                }  
            }  
        },  
        uglify: {  
            options: {  
                //compress: true  
            },  
            applib: {  
                src: [  
                    'wwwroot/lib/requirejs/require.js',
                    'wwwroot/lib/jquery/jquery.min.js',
                    'wwwroot/lib/jquery-validation/jquery.validate.js',  
                    'wwwroot/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js',  
   
                ],  
                dest: 'wwwroot/lib/vendor.js'  
            }  
        },

        bower: {
            install: {
                options: {
                    targetDir: "wwwroot/lib",
                    layout: "byComponent",
                    cleanTargetDir: false
                }
            }
        }


    });

    // This command registers the default task which will install bower packages into wwwroot/lib
    grunt.registerTask("default", ["bower:install", "typescript", "uglify", "cssmin"]);

    // The following line loads the grunt plugins.
    // This line needs to be at the end of this this file.
    grunt.loadNpmTasks("grunt-bower-task");
    grunt.loadNpmTasks('grunt-typescript');
    grunt.loadNpmTasks('grunt-tsd');
    grunt.loadNpmTasks('grunt-contrib-cssmin');
    grunt.loadNpmTasks('grunt-contrib-uglify');
};