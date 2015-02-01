// This file in the main entry point for defining grunt tasks and using grunt plugins.
// Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409

module.exports = function (grunt) {
	grunt.initConfig({
		typescript: {
			base: {
				src: ['app/ts/**/*.ts'],
				dest: 'appjs',
				options: {
					references: ['typings/tsd.d.ts'],
					module: 'amd', //or commonjs 
					target: 'es5', //or es3 
					basePath: 'app/ts',
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
                        'app/css/site.css']
				}
			}
		},
		clean: {
			jsfiles: { src: ['appjs', 'wwwroot/app'] }
		},
		copy: {
			jsfiles: {
				cwd: 'appjs',
				src: ['**'],
				dest: 'wwwroot/app/',
				expand: true
			},
			htmlfiles: {
				cwd: 'app/html',
				src: '**',
				dest: 'wwwroot/app/',
				expand: true
			},
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
                    'wwwroot/lib/breezejs/breeze.debug.js',
                    'wwwroot/lib/knockout/knockout.js',

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
		},
		tslint: { // FIXME
			options: {
				//configuration: grunt.file.readJSON("tslint.json")
			},
			files: {
				src: ['app/ts/**.ts']
			}
		},
		watch: {
			ts: {
				files: 'app/*.ts',
				tasks: ['typescript', 'copy:jsfiles'],
				options: {
					debounceDelay: 250,
				},
			},
			html: {
				files: 'app/html/**.html',
				tasks: ['typescript', 'copy:htmlfiles'],
				options: {
					debounceDelay: 250,
				},
			},
			css: {
				files: 'app/css/**.css',
				tasks: ['cssmin'],
				options: {
					debounceDelay: 250,
				},
			},
		},

	});

	// This command registers the default task which will install bower packages into wwwroot/lib
	grunt.registerTask("default", ["bower:install", "clean", "typescript", "copy"]);
	grunt.registerTask("full", ["bower:install", "clean", "typescript", "copy", "uglify", "cssmin"]);

	// The following line loads the grunt plugins.
	// This line needs to be at the end of this this file.
	grunt.loadNpmTasks("grunt-bower-task");
	grunt.loadNpmTasks('grunt-typescript');
	grunt.loadNpmTasks('grunt-tsd');
	grunt.loadNpmTasks('grunt-contrib-cssmin');
	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.loadNpmTasks('grunt-contrib-copy');
	grunt.loadNpmTasks('grunt-contrib-clean');
	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-tslint');
};