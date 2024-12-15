let globalDraggedTaskId = null;

window.taskboardInterop = {
  draggedTaskId: null,
  currentColumn: null,

  initialize: function () {
    window.addEventListener("dragstart", this.handleDragStart.bind(this));
    window.addEventListener("dragend", this.handleDragEnd.bind(this));
  },

  handleDragStart: function (e) {
    if (e.target.classList.contains("task-card")) {
      e.dataTransfer.effectAllowed = "move";
      // On stocke aussi l'ID dans l'objet dataTransfer comme backup
      e.dataTransfer.setData("text/plain", e.target.id);
      e.target.classList.add("dragging");
      document.body.classList.add("dragging");
    }
  },

  handleDragEnd: function (e) {
    if (e.target.classList.contains("task-card")) {
      e.target.classList.remove("dragging");
      document.body.classList.remove("dragging");
      this.draggedTaskId = null;
      this.currentColumn = null;
      // Force le rafraîchissement du DOM
      e.target.style.display = "none";
      e.target.offsetHeight; // Force un reflow
      e.target.style.display = "";
    }
  },

  isChildOf: function (child, parent) {
    if (!child || !parent) return false;
    let node = child.parentNode;
    while (node != null) {
      if (node === parent) {
        return true;
      }
      node = node.parentNode;
    }
    return false;
  },

  handleColumnDragEnter: function (columnElement) {
    // Si on entre dans une nouvelle colonne
    if (this.currentColumn !== columnElement) {
      this.currentColumn = columnElement;
      return true;
    }
    return false;
  },

  handleColumnDragLeave: function (columnElement) {
    const activeElement = document.activeElement;
    // Vérifie si on quitte réellement la colonne
    if (
      !this.isChildOf(activeElement, columnElement) &&
      activeElement !== columnElement
    ) {
      this.currentColumn = null;
      return true;
    }
    return false;
  },

  setDragData: function (taskId) {
    globalDraggedTaskId = `task-${taskId}`;
    return globalDraggedTaskId;
  },

  getDragData: function () {
    const result = globalDraggedTaskId;
    globalDraggedTaskId = null; // On peut réinitialiser ici après l'avoir récupéré
    return result;
  },
};

window.taskboardInterop.initialize();
