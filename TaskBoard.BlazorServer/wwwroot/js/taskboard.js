window.taskboardInterop = {
  draggedTaskId: null,

  initialize: function () {
    window.addEventListener("dragstart", this.handleDragStart.bind(this));
    window.addEventListener("dragend", this.handleDragEnd.bind(this));
  },

  handleDragStart: function (e) {
    console.log("Drag Start");
    if (e.target.classList.contains("task-card")) {
      e.dataTransfer.effectAllowed = "move";
      this.draggedTaskId = e.target.id;
      e.target.classList.add("dragging");
    }
  },

  handleDragEnd: function (e) {
    console.log("Drag End");
    if (e.target.classList.contains("task-card")) {
      e.target.classList.remove("dragging");
      this.draggedTaskId = null;
    }
  },

  setDragData: function (taskId) {
    this.draggedTaskId = `task-${taskId}`;
    console.log("Drag Data:", this.draggedTaskId);
    return taskId;
  },

  getDragData: function () {
    return this.draggedTaskId;
  },
};

// Initialize the drag and drop functionality
window.taskboardInterop.initialize();

console.log('TaskBoard JavaScript initialized');
