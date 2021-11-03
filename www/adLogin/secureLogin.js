let address = '172.30.13.46';

document.getElementById("enter").addEventListener("click", function () {

    document.cookie = '';

    let username = document.getElementById("usr").value;
    let password = document.getElementById("pwd").value;

    password = sha512(password);
    let payload = { user: username, pwd: password };

    fetch('/$getId', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)

    }).then(res => res.json()).then(data => (document.cookie = data['id']));


});
