from flask import Flask, jsonify, request
from flask_cors import CORS

app = Flask(__name__)
CORS(app)  # Permite conexión desde React

# Stats iniciales del jugador
stats = {
    "maxHealth": 100,
    "maxStamina": 100,
    "walkSpeed": 2,
    "runSpeed": 5,
    "strength": 1.5,
    "resistance": 100
}

@app.route('/stats', methods=['GET'])
def get_stats():
    return jsonify(stats)

@app.route('/stats', methods=['POST'])
def update_stats():
    data = request.get_json()
    stats.update(data)
    return jsonify({"msg": "Stats actualizados", "stats": stats})

if __name__ == '__main__':
    app.run(port=5000)
