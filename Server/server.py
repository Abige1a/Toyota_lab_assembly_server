import os
import json
from flask import Flask, request, jsonify

app = Flask(__name__)

# JSON file used as local storage
DB_FILE = os.path.join(os.path.abspath(os.path.dirname(__file__)), "data.json")
db = {}

def load_data():
    global db
    if os.path.exists(DB_FILE):
        with open(DB_FILE, "r", encoding="utf-8") as f:
            db = json.load(f)
    else:
        db = {"tasks": []}
        save_data()

def save_data():
    with open(DB_FILE, "w", encoding="utf-8") as f:
        json.dump(db, f, indent=4, ensure_ascii=False)

# ---------------- ID Generation Helper Functions ----------------

# Task IDs are globally unique.
def get_new_task_id():
    return max([task["id"] for task in db["tasks"]], default=0) + 1

# Station IDs are unique only within a given task.
def get_new_station_id_for_task(task):
    new_id = 1
    for station in task.get("stations", []):
        if station["id"] >= new_id:
            new_id = station["id"] + 1
    return new_id

# Page IDs are unique only within a given station.
def get_new_page_id_for_station(station):
    new_id = 1
    for page in station.get("pages", []):
        if page["id"] >= new_id:
            new_id = page["id"] + 1
    return new_id

# Text IDs are unique only within a given page.
def get_new_text_id_for_page(page):
    new_id = 1
    for text in page.get("texts", []):
        if text["id"] >= new_id:
            new_id = text["id"] + 1
    return new_id

# Image IDs are unique only within a given page.
def get_new_image_id_for_page(page):
    new_id = 1
    for image in page.get("images", []):
        if image["id"] >= new_id:
            new_id = image["id"] + 1
    return new_id

# ---------------- Helper Functions to Locate Objects ----------------

# Find a task by its id (global)
def find_task(task_id):
    for task in db["tasks"]:
        if task["id"] == task_id:
            return task
    return None

# Find a station within a given task by station id.
def find_station_in_task(task, station_id):
    for station in task.get("stations", []):
        if station["id"] == station_id:
            return station
    return None

# Find a page within a given station by page id.
def find_page_in_station(station, page_id):
    for page in station.get("pages", []):
        if page["id"] == page_id:
            return page
    return None

# ---------------------- "All Tasks" Endpoint ------------------------
# Returns the complete data (all tasks with their nested data)
@app.route('/all_tasks', methods=['GET'])
def get_all_tasks():
    return jsonify(db)

# ---------------------- Task Endpoints ------------------------

@app.route('/tasks', methods=['POST'])
def create_task():
    data = request.get_json()
    if not data or "name" not in data:
        return jsonify({"error": "Task name is required"}), 400
    new_id = get_new_task_id()
    new_task = {"id": new_id, "name": data["name"], "stations": []}
    db["tasks"].append(new_task)
    save_data()
    return jsonify({"message": "Task created successfully", "task_id": new_id}), 201

@app.route('/tasks', methods=['GET'])
def get_tasks():
    tasks_list = [{"id": t["id"], "name": t["name"]} for t in db["tasks"]]
    return jsonify(tasks_list)

@app.route('/tasks/<int:task_id>', methods=['GET'])
def get_task(task_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    return jsonify(task)

@app.route('/tasks/<int:task_id>', methods=['PUT'])
def update_task(task_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    data = request.get_json()
    if not data or "name" not in data:
        return jsonify({"error": "Task name is required"}), 400
    task["name"] = data["name"]
    save_data()
    return jsonify({"message": "Task updated successfully"})

@app.route('/tasks/<int:task_id>', methods=['DELETE'])
def delete_task(task_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    db["tasks"].remove(task)
    save_data()
    return jsonify({"message": "Task deleted successfully"})

# --------------------- Station Endpoints ------------------------

# Create a new Station under a specific Task
@app.route('/tasks/<int:task_id>/stations', methods=['POST'])
def create_station(task_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    data = request.get_json()
    if not data or "name" not in data:
        return jsonify({"error": "Station name is required"}), 400
    new_station_id = get_new_station_id_for_task(task)
    new_station = {"id": new_station_id, "name": data["name"], "pages": []}
    task.setdefault("stations", []).append(new_station)
    save_data()
    return jsonify({"message": "Station created successfully", "station_id": new_station_id}), 201

# Delete a Station under a Task
@app.route('/tasks/<int:task_id>/stations/<int:station_id>', methods=['DELETE'])
def delete_station(task_id, station_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    station = find_station_in_task(task, station_id)
    if not station:
        return jsonify({"error": "Station not found"}), 404
    task["stations"].remove(station)
    save_data()
    return jsonify({"message": "Station deleted successfully"})

# --------------------- Page Endpoints ------------------------

# Create a new Page under a specific Station
@app.route('/tasks/<int:task_id>/stations/<int:station_id>/pages', methods=['POST'])
def create_page(task_id, station_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    station = find_station_in_task(task, station_id)
    if not station:
        return jsonify({"error": "Station not found"}), 404
    data = request.get_json() or {}
    layout_template_index = data.get("layout_template_index", 0)
    new_page_id = get_new_page_id_for_station(station)
    new_page = {
        "id": new_page_id,
        "layout_template_index": layout_template_index,
        "texts": [],
        "images": []
    }
    station.setdefault("pages", []).append(new_page)
    save_data()
    return jsonify({"message": "Page created successfully", "page_id": new_page_id}), 201

# Delete a Page under a specific Station
@app.route('/tasks/<int:task_id>/stations/<int:station_id>/pages/<int:page_id>', methods=['DELETE'])
def delete_page(task_id, station_id, page_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    station = find_station_in_task(task, station_id)
    if not station:
        return jsonify({"error": "Station not found"}), 404
    page = find_page_in_station(station, page_id)
    if not page:
        return jsonify({"error": "Page not found"}), 404
    station["pages"].remove(page)
    save_data()
    return jsonify({"message": "Page deleted successfully"})

# Update a Page's layout_template_index
@app.route('/tasks/<int:task_id>/stations/<int:station_id>/pages/<int:page_id>', methods=['PUT'])
def update_page(task_id, station_id, page_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    station = find_station_in_task(task, station_id)
    if not station:
        return jsonify({"error": "Station not found"}), 404
    page = find_page_in_station(station, page_id)
    if not page:
        return jsonify({"error": "Page not found"}), 404
    data = request.get_json()
    if "layout_template_index" in data:
        page["layout_template_index"] = data["layout_template_index"]
    save_data()
    return jsonify({"message": "Page updated successfully"})

# ------------------- Page Text Endpoints ---------------------

# Add a text segment to a Page
@app.route('/tasks/<int:task_id>/stations/<int:station_id>/pages/<int:page_id>/texts', methods=['POST'])
def add_text(task_id, station_id, page_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    station = find_station_in_task(task, station_id)
    if not station:
        return jsonify({"error": "Station not found"}), 404
    page = find_page_in_station(station, page_id)
    if not page:
        return jsonify({"error": "Page not found"}), 404
    data = request.get_json()
    if not data or "content" not in data:
        return jsonify({"error": "Text content is required"}), 400
    order = data.get("order", 0)
    new_text_id = get_new_text_id_for_page(page)
    new_text = {"id": new_text_id, "content": data["content"], "order": order}
    page.setdefault("texts", []).append(new_text)
    save_data()
    return jsonify({"message": "Text segment added successfully", "text_id": new_text_id}), 201

@app.route('/tasks/<int:task_id>/stations/<int:station_id>/pages/<int:page_id>/texts/<int:text_id>', methods=['PUT'])
def update_text(task_id, station_id, page_id, text_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    station = find_station_in_task(task, station_id)
    if not station:
        return jsonify({"error": "Station not found"}), 404
    page = find_page_in_station(station, page_id)
    if not page:
        return jsonify({"error": "Page not found"}), 404
    texts = page.get("texts", [])
    text_segment = None
    for t in texts:
        if t["id"] == text_id:
            text_segment = t
            break
    if not text_segment:
        return jsonify({"error": "Text segment not found"}), 404
    data = request.get_json()
    if "content" in data:
        text_segment["content"] = data["content"]
    if "order" in data:
        text_segment["order"] = data["order"]
    save_data()
    return jsonify({"message": "Text segment updated successfully"})

@app.route('/tasks/<int:task_id>/stations/<int:station_id>/pages/<int:page_id>/texts/<int:text_id>', methods=['DELETE'])
def delete_text(task_id, station_id, page_id, text_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    station = find_station_in_task(task, station_id)
    if not station:
        return jsonify({"error": "Station not found"}), 404
    page = find_page_in_station(station, page_id)
    if not page:
        return jsonify({"error": "Page not found"}), 404
    texts = page.get("texts", [])
    text_segment = None
    for t in texts:
        if t["id"] == text_id:
            text_segment = t
            break
    if not text_segment:
        return jsonify({"error": "Text segment not found"}), 404
    texts.remove(text_segment)
    save_data()
    return jsonify({"message": "Text segment deleted successfully"})

# ------------------- Page Image Endpoints ---------------------

@app.route('/tasks/<int:task_id>/stations/<int:station_id>/pages/<int:page_id>/images', methods=['POST'])
def add_image(task_id, station_id, page_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    station = find_station_in_task(task, station_id)
    if not station:
        return jsonify({"error": "Station not found"}), 404
    page = find_page_in_station(station, page_id)
    if not page:
        return jsonify({"error": "Page not found"}), 404
    data = request.get_json()
    if not data or "data" not in data:
        return jsonify({"error": "Image data is required"}), 400
    order = data.get("order", 0)
    new_image_id = get_new_image_id_for_page(page)
    new_image = {"id": new_image_id, "data": data["data"], "order": order}
    page.setdefault("images", []).append(new_image)
    save_data()
    return jsonify({"message": "Image added successfully", "image_id": new_image_id}), 201

@app.route('/tasks/<int:task_id>/stations/<int:station_id>/pages/<int:page_id>/images/<int:image_id>', methods=['PUT'])
def update_image(task_id, station_id, page_id, image_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    station = find_station_in_task(task, station_id)
    if not station:
        return jsonify({"error": "Station not found"}), 404
    page = find_page_in_station(station, page_id)
    if not page:
        return jsonify({"error": "Page not found"}), 404
    images = page.get("images", [])
    image = None
    for i in images:
        if i["id"] == image_id:
            image = i
            break
    if not image:
        return jsonify({"error": "Image not found"}), 404
    data = request.get_json()
    if "data" in data:
        image["data"] = data["data"]
    if "order" in data:
        image["order"] = data["order"]
    save_data()
    return jsonify({"message": "Image updated successfully"})

@app.route('/tasks/<int:task_id>/stations/<int:station_id>/pages/<int:page_id>/images/<int:image_id>', methods=['DELETE'])
def delete_image(task_id, station_id, page_id, image_id):
    task = find_task(task_id)
    if not task:
        return jsonify({"error": "Task not found"}), 404
    station = find_station_in_task(task, station_id)
    if not station:
        return jsonify({"error": "Station not found"}), 404
    page = find_page_in_station(station, page_id)
    if not page:
        return jsonify({"error": "Page not found"}), 404
    images = page.get("images", [])
    image = None
    for i in images:
        if i["id"] == image_id:
            image = i
            break
    if not image:
        return jsonify({"error": "Image not found"}), 404
    images.remove(image)
    save_data()
    return jsonify({"message": "Image deleted successfully"})

# ---------------------- Load Data and Start the Server ------------------------

# @app.before_first_request
# def initialize():
#     load_data()

if __name__ == '__main__':
    load_data()  # Ensure data is loaded before starting the server
    app.run(debug=True, host='0.0.0.0', port=5000)
