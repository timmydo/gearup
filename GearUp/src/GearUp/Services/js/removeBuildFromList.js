


function removeBuildFromList(buildid, listid, uid) {
	var context = getContext();
	var collection = context.getCollection();
	var collectionLink = collection.getSelfLink();
	var response = context.getResponse();
	var queryspec = {
		query: "SELECT * FROM lists l WHERE l.id=@id",
		parameters: [{ name: '@id', value: listid }]
	};

	collection.queryDocuments(collectionLink, queryspec, {}, 
		function (err, documents, responseOptions) {
			if (err) throw new Error("Error " + err.message);
			if (!documents || documents.length < 1) {
				throw "Remove build from list, Document not found: " + listid
			}
			var list = documents[0];
			if (list.Creator !== uid) {
				throw "list modifier is not the creator of this list"
			}
			if (list.Builds.indexOf(buildid) === -1) {
				throw "list does not contain build";
			}
			for (var i = list.Builds.length - 1; i >= 0; i--) {
				if (list.Builds[i] === buildid) {
					list.Builds.splice(i, 1);
				}
			}
			collection.replaceDocument(list._self, list,
				function (err, docReplaced) {
					if (err) {
						throw "Cannot replace list";
					} else {
						response.setBody(JSON.stringify(list));
					}
				});
		});


}
