let error = true

let res = [
    db.container.drop(),
    db.container.createIndex({
        name: 1
    }, {
        unique: true
    }),
    db.container.createIndex({
        thatfield: 1
    }),
    db.container.insert({
        myfield: 'hello',
        thatfield: 'testing'
    }),
    db.container.insert({
        myfield: 'hello2',
        thatfield: 'testing'
    })
]
error = false;
printjson(res)


// db.createUser(
//     {
//         user: "<user for database which shall be created>",
//         pwd: "<password of user>",
//         roles: [
//             {
//                 role: "readWrite",
//                 db: "<database to create>"
//             }
//         ]
//     }
// );

if (error) {
    print('Error, exiting' + error)
    quit(1)
}