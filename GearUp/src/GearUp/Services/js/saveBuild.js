


function saveBuild(json) {
	var context = getContext();
	var collection = context.getCollection();
	var collectionLink = collection.getSelfLink();
	var response = context.getResponse();
	var newBuild = JSON.parse(json);
	if (!newBuild.id) {
		throw "Build ID is missing";
	}
	var queryspec = {
		query: "SELECT * FROM builds b WHERE b.id=@id",
		parameters: [{ name: '@id', value: newBuild.id }]
	};

	collection.queryDocuments(collectionLink, queryspec, {},
		function (err, documents, responseOptions) {
			if (err) throw new Error("Error " + err.message);
			if (!documents || documents.length < 1) {
				throw "Document not found, id: " + newBuild.id
			}
			var build = documents[0];
			build.Modified = Date.now();
			build.Version = newBuild.Version;
			build.Title = newBuild.Title;
			build.Url = newBuild.Url;
			build.Description = newBuild.Description;
			build.Parts = newBuild.Parts;
			collection.replaceDocument(build._self, build,
				function (err, docReplaced) {
					if (err) {
						throw "Cannot replace build";
					} else {
						response.setBody(newBuild.id);
					}
				});
		});


}
