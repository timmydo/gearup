// This file in the main entry point for defining grunt tasks and using grunt plugins.
// Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409

module.exports = function (grunt) {
	grunt.initConfig({
		typescript: {
			debug: {
				src: ['app/ts/**/*.ts'],
				dest: 'wwwroot/app/app.js',
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
			vendorcss: {
				files: {
					'wwwroot/css/bootstrap.css': ['bower_components/bootstrap/dist/css/bootstrap.min.css'],
					'wwwroot/css/font-awesome.css': ['bower_components/Font-Awesome/css/font-awesome.min.css']
				}

			},
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
			files: { src: ['appjs', 'wwwroot/app', 'wwwroot/lib', 'wwwroot/css'] }
		},
		copy: {
			ember: {
				cwd: 'bower_components/ember',
				src: 'ember.js',
				dest: 'wwwroot/lib/',
				expand: true
			}
		},
		uglify: {
			options: {
				//compress: true  
			},
			applib: {
				src: [
                    'bower_components/jquery/jquery.min.js',
                    'bower_components/jquery-validation/jquery.validate.js',
                    'bower_components/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js',
                    'bower_components/bootstrap/dist/js/bootstrap.min.js',
                    'bower_components/handlebars/handlebars.min.js',
                    'bower_components/ember/ember.min.js',

				],
				dest: 'wwwroot/lib/vendor.js'
			}
		},

		/*
		bower: {
			install: {
				options: {
					targetDir: "wwwroot/lib",
					layout: "byComponent",
					cleanTargetDir: false
				}
			}
		},
		*/
		tslint: { // FIXME
			options: {
				//configuration: grunt.file.readJSON("tslint.json")
			},
			files: {
				src: ['app/ts/**.ts']
			}
		},
		emberTemplates: {
			compile: {
				options: {
					templateFileExtensions: /\.html/,
					//amd: true,
					templateBasePath: 'app/html',
					
				},
				files: {
					"wwwroot/app/templates.js": ["app/html/*.html"]
				}
			}
		},
		watch: {
			ts: {
				files: 'app/ts/*.ts',
				tasks: ['typescript', 'copy:jsfiles'],
				options: {
					debounceDelay: 250,
				},
			},
			html: {
				files: 'app/html/**.html',
				tasks: ['emberTemplates'],
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
			}
		},

	});

	// This command registers the default task which will install bower packages into wwwroot/lib
	grunt.registerTask("default", ["clean", "typescript", "copy", "uglify", "cssmin", "emberTemplates"]);

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
	grunt.loadNpmTasks('grunt-ember-templates');

};