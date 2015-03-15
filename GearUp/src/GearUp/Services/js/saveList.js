


function saveList(json, uid) {
	var context = getContext();
	var collection = context.getCollection();
	var collectionLink = collection.getSelfLink();
	var response = context.getResponse();
	var newList = JSON.parse(json);
	if (!newList.id) {
		throw "List ID is missing";
	}
	var queryspec = {
		query: "SELECT * FROM lists l WHERE l.id=@id",
		parameters: [{ name: '@id', value: newList.id }]
	};

	collection.queryDocuments(collectionLink, queryspec, {},
		function (err, documents, responseOptions) {
			if (err) throw new Error("Error " + err.message);
			if (!documents || documents.length < 1) {
				throw "Document not found, id: " + newList.id
			}
			var list = documents[0];
			if (list.Creator !== uid) {
				throw "list modifier is not the creator of this list"
			}
			list.Modified = (new Date()).toISOString();
			list.Version = newList.Version;
			list.Title = newList.Title;
			list.Description = newList.Description;
			list.Builds = newList.Builds;
			collection.replaceDocument(list._self, list,
				function (err, docReplaced) {
					if (err) {
						throw "Cannot replace list";
					} else {
						response.setBody(newList.id);
					}
				});
		});


}
