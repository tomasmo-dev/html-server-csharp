let username;

function getUsername() {
    fetch('/$getUsername', {
        method: 'POST',
        headers:{
            'Accept': 'application/json'
        },
        body: JSON.stringify({ cookies: document.cookie })
    }).then(res => res.json()).then(data => function(data){
            username = data['usr'];
            document.getElementById('welcomeHeader').innerHTML = 'Welcome ' + username + '!'
    });
}