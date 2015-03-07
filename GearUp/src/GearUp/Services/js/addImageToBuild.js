


function addImageToBuild(buildid, imageid, uid) {
	var context = getContext();
	var collection = context.getCollection();
	var collectionLink = collection.getSelfLink();
	var response = context.getResponse();
	var queryspec = {
		query: "SELECT * FROM builds b WHERE b.id=@id",
		parameters: [{ name: '@id', value: buildid }]
	};

	collection.queryDocuments(collectionLink, queryspec, {}, 
		function (err, documents, responseOptions) {
			if (err) throw new Error("Error " + err.message);
			if (!documents || documents.length < 1) {
				throw "Add image to build, Document not found: " + buildid
			}
			var build = documents[0];
			if (build.Creator !== uid) {
				throw "Build modifier is not the creator of this build"
			}
			build.Images = build.Images.concat({ 'guid': imageid });
			collection.replaceDocument(build._self, build,
				function (err, docReplaced) {
					if (err) {
						throw "Cannot replace build";
					} else {
						response.setBody(JSON.stringify(build));
					}
				});
		});


}
