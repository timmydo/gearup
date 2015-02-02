
Readme

todo - cache busting/versioning


Data/Models
---------------

partType: string?

img: {
	iid: guid
	url
	description: string
	uploaded: date
}

part: {
    pid: guid
	creator: user
	title: string
	description: string
	mfrLink: url
	buyLink: [url...]
	price: money
	type: partType
	img: [img...]
}

build: {
    bid: guid
	modified: date
    created: date
	creator: user
    
    whole: part?

    parts: [part...]
}

user: {
	name: string
	email: string
	idprovider: blah
}

